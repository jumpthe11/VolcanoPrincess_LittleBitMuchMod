using HarmonyLib;

namespace StrictlyBusiness;

[HarmonyPatch(typeof(ItemSys), "AddShopFavor")]
public static class Patch
{
	private static bool Prefix()
	{
		Person tempNpc = NpcSys.tempNpc;
		bool flag = false;
		return false;
	}
}
