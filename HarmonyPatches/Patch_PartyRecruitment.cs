using HarmonyLib;
using PartySizeReunited.McMMenu.Options;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;

namespace PartySizeReunited.HarmonyPatches
{
    [HarmonyPatch(typeof(DefaultClanTierModel), "GetPartyLimitForTier")]
    internal class Patch_PartyRecruitment
    {
        public static void Postfix(Clan clan, int clanTierToCheck, ref int __result)
        {
            if (SubModule.PartyRecruitmentOptions.IsActivate && clan == Clan.PlayerClan && clan.Leader.IsHumanPlayerCharacter)
            {
                if (SubModule.PartyRecruitmentOptions.Type.SelectedValue.SelectedValue == OptionTypeEnum.Static)
                {
                    __result = SubModule.PartyRecruitmentOptions.Amount;
                }
                else if (SubModule.PartyRecruitmentOptions.Type.SelectedValue.SelectedValue == OptionTypeEnum.Progressive)
                {
                    __result = clanTierToCheck + SubModule.PartyRecruitmentOptions.Amount;
                }
            }
        }
    }
}
