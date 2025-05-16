using MelonLoader;
using HarmonyLib;
using UnityEngine;
using static MelonLoader.MelonLogger;
using System.Reflection;
using UnityEngine.UIElements;

[assembly: MelonInfo(typeof(LittleBitMuchAdjustment.Core), "LittleBitMuchAdjustment", "1.0.0", "jumpthe11", null)]
[assembly: MelonGame("EggHatcher", "VolcanoPrincess")]

namespace LittleBitMuchAdjustment
{
    public class Core : MelonMod
    {
        // Fields to hold references to the private fields
        private FieldInfo horseMilkField;
        private FieldInfo horseProductionField;

        public int milkMultiplier = 10;  // Multiply milk production by this value
        public int coinMultiplier = 10;  // Multiply coin production by this value
        public int lightFruitBonus = 5;  // Get this many light fruits instead of 1

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Lets Go.");



            var harmony = new HarmonyLib.Harmony("com.musa.mod");


            // Modify static fields directly
            Constant.waterCost = 1;
            Constant.defaultEnergy = 100;
            Constant.doubleUnit = 500;
            Constant.exploCost = 2;
            Constant.doubleEnemyItemProb = 95;
            Constant.maxHorseNum = 500;

            Constant.ownItemNum = 50;
            Constant.feedAddFavor = 50;
            Constant.feedHorsePrice = 500;
            Constant.maxChatNum = 40;
            Constant.maxHorseFavor = 500;
            Constant.raceHorseNum = 10;

            Constant.defaultSeed = 50;
            Constant.fishingCost = 2;
            MelonLogger.Msg($"waterCost set to: {Constant.waterCost}");
            MelonLogger.Msg($"defaultEnergy set to: {Constant.defaultEnergy}");
            horseMilkField = AccessTools.Field(typeof(HorseSys), "horseMilk");
            horseProductionField = AccessTools.Field(typeof(HorseSys), "horseProduction");
            if (horseMilkField == null || horseProductionField == null)
            {
                MelonLogger.Error("Failed to get reflection references to private fields!");
            }
            else
            {
                MelonLogger.Msg("Successfully initialized reflection for private fields");
            }

            harmony.PatchAll(); // Apply all Harmony patches automatically

            MelonLogger.Msg("Harmony Patches Applied!");
            MelonLogger.Msg($"Production boost settings: Milk ×{milkMultiplier}, Coins ×{coinMultiplier}, Light Fruit ×{lightFruitBonus}");

        }

        bool isTalentScreenActive = false; // Track if player is in talent screen

        public override void OnUpdate()
        {

            if (Input.GetKeyDown(KeyCode.T)) // Press "T" to toggle talent screen state
            {
                isTalentScreenActive = !isTalentScreenActive;
                MelonLogger.Msg($"Talent Screen Active: {isTalentScreenActive}");
            }

            if (Input.GetKeyDown(KeyCode.R) && isTalentScreenActive) // Ensure screen is active
            {
                var dataCfgField = AccessTools.Field(typeof(DataSys), "dataCfg");
                var dataCfg = (DataConfig)dataCfgField.GetValue(DataSys.Instan); // Explicit cast


                ResetAllTalents();
            }
            if (Input.GetKeyDown(KeyCode.P)) // Press "P" to add points lol
            {
                AddTalentPoint(20); // Adjust the number if you want more

            }

            // Press F10 to collect all horse production and bypass UI
            if (Input.GetKeyDown(KeyCode.F10))
            {
                CollectAllHorseProduction();
            }


            // Press F5 to modify the production multipliers
            if (Input.GetKeyDown(KeyCode.F5))
            {
                milkMultiplier += 10;
                coinMultiplier += 10;
                lightFruitBonus += 5;
                MelonLogger.Msg($"Increased production multipliers: Milk ×{milkMultiplier}, Coins ×{coinMultiplier}, Light Fruit ×{lightFruitBonus}");
            }
        }

        
        private void CollectAllHorseProduction()
        {
            // Get reference to HorseSys instance
            HorseSys horseSys = HorseSys.Instan;

            if (horseSys == null)
            {
                MelonLogger.Error("HorseSys.Instan is null!");
                return;
            }

            if (horseMilkField == null || horseProductionField == null)
            {
                MelonLogger.Error("Private field references not initialized!");
                return;
            }

            // Get the private arrays using reflection
            int[] horseMilk = (int[])horseMilkField.GetValue(horseSys);
            int[] horseProduction = (int[])horseProductionField.GetValue(horseSys);

            if (horseMilk == null || horseProduction == null)
            {
                MelonLogger.Error("Failed to get horseMilk or horseProduction arrays!");
                return;
            }

            int totalMilk = 0;
            int totalLightFruit = 0;
            int totalCoins = 0;

            // Loop through all horses and collect their production
            for (int i = 0; i < horseSys.ownHorse.Count; i++)
            {
                // Collect milk
                if (horseMilk[i] > 0)
                {
                    int milkAmount = horseMilk[i];
                    totalMilk += milkAmount;
                    horseSys.ownHorse[i].milkNum += milkAmount;
                    horseMilk[i] = 0;
                }

                // Collect production (light fruit or coins)
                if (horseProduction[i] != 0)
                {
                    if (horseProduction[i] == 2)
                    {
                        totalLightFruit++;
                    }
                    else
                    {
                        totalCoins += horseProduction[i];
                    }
                    horseProduction[i] = 0;
                }
            }

            // Add all collected items to inventory at once
            if (totalMilk > 0)
            {
                totalMilk *= milkMultiplier;
                ItemSys.AddItem(Constant.horseMilkIndex, totalMilk, true);
                MelonLogger.Msg($"Collected {totalMilk} horse milk!");
            }

            if (totalLightFruit > 0)
            {
                totalLightFruit *= lightFruitBonus;
                ItemSys.AddItem(Constant.lightFruit, totalLightFruit, true);
                MelonLogger.Msg($"Collected {totalLightFruit} light fruit!");
            }

            if (totalCoins > 0)
            {
                totalCoins *= coinMultiplier;
                DataSys.AddCoin(BillType.stableIncome, totalCoins);
                MelonLogger.Msg($"Collected {totalCoins} coins from horse production!");
            }

            // Update the private fields with our modified arrays
            horseMilkField.SetValue(horseSys, horseMilk);
            horseProductionField.SetValue(horseSys, horseProduction);

            // Play sound and update UI
            AudioSys.Instan.PlaySound(23);
            horseSys.UpdateStableUi();

            MelonLogger.Msg("All horse production collected successfully!");
        }

        public static void AddTalentPoint(int num)
        {
            if (DauSys.Instan == null)
            {
                MelonLogger.Warning("Instan is null! Cannot add talent points.");
                return;
            }

            DauSys.AddTalentPoint(num);
            MelonLogger.Msg($"Talent points increased by {num}.");
        }
        private int tempTalentPointToRefund = 0;

        public void ResetAllTalents()
        {
           
            if (DataSys.Instan == null)
            {
                MelonLogger.Warning("DataSys.Instan is null! Cannot reset talents.");
                return;
            }

            var dataCfgField = AccessTools.Field(typeof(DataSys), "dataCfg");
            var dataCfg = (DataConfig)dataCfgField.GetValue(DataSys.Instan);

            if (dataCfg == null || dataCfg.talents == null)
            {
                MelonLogger.Warning("dataCfg or talents list is null!");
                return;
            }

            var updateMethod = AccessTools.Method(typeof(DauSys), "UpdateTalentUi");
            updateMethod.Invoke(DauSys.Instan, null);
            if (updateMethod == null)
            {
                MelonLogger.Warning("Method 'UpdateTalentUi' not found in UiSys!");
                return;
            }

            AudioSys.Instan.PlaySound(11);

            foreach (Talent talent in dataCfg.talents)
            {
                if (talent == null)
                {
                    MelonLogger.Warning("A talent in dataCfg.talents is null!");
                    continue;
                }
                if (talent.level != 0)
                {
                    tempTalentPointToRefund++;
                    talent.level = 0;  // Reset talent level
                }

            }

            DauSys.AddTalentPoint(tempTalentPointToRefund); // Correct method for talent points
            MelonLogger.Msg($"✅ Added {tempTalentPointToRefund} Talent Points!");
            tempTalentPointToRefund = 0;
            MelonLogger.Msg($"Talent point zeroed i hope {tempTalentPointToRefund} Talent Points!");

            MelonLogger.Msg("✅ All talents have been reset, and talent points restored!");
        }





    }
}