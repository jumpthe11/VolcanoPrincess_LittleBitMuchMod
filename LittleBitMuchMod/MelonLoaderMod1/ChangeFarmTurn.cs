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
