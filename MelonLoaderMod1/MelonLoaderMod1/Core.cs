using MelonLoader;
using HarmonyLib;
using UnityEngine;
using static MelonLoader.MelonLogger;
using System.Reflection;

[assembly: MelonInfo(typeof(MelonLoaderMod1.Core), "MelonLoaderMod1", "1.0.0", "Musa-", null)]
[assembly: MelonGame("EggHatcher", "VolcanoPrincess")]

namespace LittleBitMuchAdjustment
{
    public class Core : MelonMod
    {
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
            Constant.defaultSeed = 50;
            Constant.fishingCost = 2;
            MelonLogger.Msg($"waterCost set to: {Constant.waterCost}");
            MelonLogger.Msg($"defaultEnergy set to: {Constant.defaultEnergy}");

            harmony.PatchAll(); // Apply all Harmony patches automatically

            MelonLogger.Msg("Harmony Patches Applied!");
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
                MelonLogger.Msg("✅ Added 10 Talent Points!");
            }
            if (Input.GetKeyDown(KeyCode.P)) // Press "P" to add points lol
            {
                AddTalentPoint(20); // Adjust the number if you want more

            }

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

                talent.level = 0;  // Reset talent level
            }

            DauSys.AddTalentPoint(10); // Correct method for talent points

            MelonLogger.Msg("✅ All talents have been reset, and talent points restored!");
        }





    }
}