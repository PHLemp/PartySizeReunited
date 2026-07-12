using HarmonyLib;
using PartySizeReunited.McMMenu.Options;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;

namespace PartySizeReunited.HarmonyPatches
{
    [HarmonyPatch(typeof(DefaultClanTierModel), "GetCompanionLimit")]
    class Patch_Companion
    {
        public static void Postfix(Clan clan, DefaultClanTierModel __instance, ref int __result)
        {
            if (SubModule.CompanionsOptions.IsActivate && clan.Leader.IsHumanPlayerCharacter)
            {
                if (SubModule.CompanionsOptions.Type.SelectedValue.SelectedValue == OptionTypeEnum.Static)
                {
                    __result = SubModule.CompanionsOptions.Amount;
                }
                else if (SubModule.CompanionsOptions.Type.SelectedValue.SelectedValue == OptionTypeEnum.Progressive)
                {
                    __result += SubModule.CompanionsOptions.Amount;
                }
            }
        }
    }
}
