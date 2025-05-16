using HarmonyLib;
using MelonLoader;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace LittleBitMuchAdjustment
{
    public enum TalentEnum
    {
        atk,
        maxHp,
        def,
        cri,
        avoid,
        doubleEnemyItem,
        farm,
        cooking,
        horseAdd,
        energy,
        diceCoin,
        seedNum,
        chat,
        sendGift,
        bathWork,
        doubleBattleItem,
        buy,
        date,
        exploCoinDouble,
        alchemyRate,
        dateAtmosphere,
    }



    [HarmonyPatch(typeof(ItemSys), "RareFoodRate")]
    public static class Patch_RareFoodRate
    {
        static void Postfix(ref int __result)
        {
            __result += 200; // Boost rare food rate
        }
    }
    [HarmonyPatch(typeof(ItemSys), "UseFertilizerBtn")]
    public static class Patch_UseFertilizerBtn
    {
        static bool Prefix(object __instance)
        {
            // Remove one fertilizer using static method
            ItemSys.AddItem(Constant.fertilizerIndex, -1);

            // Use reflection to call private methods
            var addFarmPrg = AccessTools.Method(__instance.GetType(), "AddFarmPrg");
            var updateFarmUi = AccessTools.Method(__instance.GetType(), "UpdateFarmUi");

            if (addFarmPrg != null)
                addFarmPrg.Invoke(__instance, new object[] { 100 });  // set to 100 progress

            if (updateFarmUi != null)
                updateFarmUi.Invoke(__instance, null);

            MelonLogger.Msg("Fertilizer used — farm progress set to 100!");

            // Skip original method
            return false;
        }
    }

    [HarmonyPatch(typeof(DauSys), "SureAddTalent")]
    public static class Patch_SureAddTalent
    {
        static void Postfix()
        {

            try
            {
                // Ensure the instance is not null
                var dauSys = DauSys.Instan;
                if (dauSys == null)
                {
                    MelonLogger.Warning("DauSys.Instan is null.");
                    return;
                }

                // Get the private tempChooseTalent field
                var tempTalentField = typeof(DauSys).GetField("tempChooseTalent", BindingFlags.NonPublic | BindingFlags.Instance);
                if (tempTalentField == null)
                {
                    MelonLogger.Warning("tempChooseTalent field not found.");
                    return;
                }

                int tempTalent = (int)tempTalentField.GetValue(dauSys);
                MelonLogger.Msg($"Chosen Talent Index: {tempTalent}");

                // Enum indexes from TalentEnum
                const int ENERGY = 9;
                const int SEEDNUM = 11;
                // Only apply once per correct talent choice
                if (tempTalent == ENERGY)
                {
  
                    MelonLogger.Msg("✅ Max Energy Talent picked! +50 Energy added.");
                    MelonLogger.Msg($"Is DataSys.Instan null? {DataSys.Instan}");

                    Constant.defaultEnergy += 50;

                }
                else if (tempTalent == SEEDNUM)
                {
                    if (ItemSys.Instan != null)
                    {
                        ItemSys.Instan.maxSeed += 20;
                        MelonLogger.Msg("🌱 SeedNum Talent picked! +5 Max Seed.");
                    }
                    else
                    {
                        MelonLogger.Warning("ItemSys.Instan is null.");
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[SureAddTalent Patch Error] {ex}");
            }
        }

    }
     // Advanced version - automatically collect every game turn
        [HarmonyPatch(typeof(HorseSys), "TurnHorseIncome")]
        public class TurnHorseIncomePatch
        {
            [HarmonyPostfix]
            public static void Postfix(HorseSys __instance)
            {
                // This will automatically collect all production after the game generates it
                // Uncomment this if you want automatic collection every turn
                /*
                // Get references to private fields
                FieldInfo horseMilkField = AccessTools.Field(typeof(HorseSys), "horseMilk");
                FieldInfo horseProductionField = AccessTools.Field(typeof(HorseSys), "horseProduction");
                
                if (horseMilkField == null || horseProductionField == null)
                {
                    MelonLogger.Error("Failed to get reflection references in patch!");
                    return;
                }
                
                int[] horseMilk = (int[])horseMilkField.GetValue(__instance);
                int[] horseProduction = (int[])horseProductionField.GetValue(__instance);
                
                if (horseMilk == null || horseProduction == null || __instance.ownHorse.Count == 0)
                {
                    return;
                }
                
                int totalMilk = 0;
                int totalLightFruit = 0;
                int totalCoins = 0;
                
                // Loop through all horses and collect production
                for (int i = 0; i < __instance.ownHorse.Count; i++)
                {
                    // Collect milk
                    if (horseMilk[i] > 0)
                    {
                        totalMilk += horseMilk[i];
                        __instance.ownHorse[i].milkNum += horseMilk[i];
                        horseMilk[i] = 0;
                    }
                    
                    // Collect production
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
                
                // Add all collected items
                if (totalMilk > 0)
                    ItemSys.AddItem(Constant.horseMilkIndex, totalMilk, true);
                
                if (totalLightFruit > 0)
                    ItemSys.AddItem(Constant.lightFruit, totalLightFruit, true);
                
                if (totalCoins > 0)
                    DataSys.AddCoin(BillType.stableIncome, totalCoins);
                
                // Update the private fields with our modified arrays
                horseMilkField.SetValue(__instance, horseMilk);
                horseProductionField.SetValue(__instance, horseProduction);
                
                MelonLogger.Msg($"Auto-collected: {totalMilk} milk, {totalLightFruit} light fruit, {totalCoins} coins");
                
                // Update UI
                __instance.UpdateStableUi();
                */
            }
        }





    


    [HarmonyPatch(typeof(MapSys), "DoingSthEnu")]
    public static class Patch_DoingSthEnu
    {
        static IEnumerator Postfix(IEnumerator __result)
        {
            // Let the original coroutine run first
            while (__result.MoveNext())
            {
                yield return __result.Current;
            }

            var selected = MapSys.tempBtn?.enName;
            if (selected == "stump" || selected == "bed")
            {
                int bonus = 30;

                // Check if energy is not already at max before adding
                if (DataSys.energy < Constant.defaultEnergy)
                {
                    DataSys.AddEnergy(bonus);
                    MelonLogger.Msg($"[MOD] Gained extra {bonus} energy from {selected}");
                }
                else
                {
                    MelonLogger.Msg("[MOD] Energy is already full, no bonus added.");
                }
            }
        }
    }




}
