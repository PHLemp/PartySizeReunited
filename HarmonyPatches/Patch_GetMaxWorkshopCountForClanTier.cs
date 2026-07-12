using HarmonyLib;
using TaleWorlds.CampaignSystem.GameComponents;

namespace PartySizeReunited.HarmonyPatches
{
    [HarmonyPatch(typeof(DefaultWorkshopModel), nameof(DefaultWorkshopModel.GetMaxWorkshopCountForClanTier))]
    internal class Patch_GetMaxWorkshopCountForClanTier
    {
        public static void Postfix(ref int __result)
        {
            if (!SubModule.WorkshopLimitOptions.IsActivate)
            {
                return;
            }

            __result += SubModule.WorkshopLimitOptions.Amount;
        }
    }
}


