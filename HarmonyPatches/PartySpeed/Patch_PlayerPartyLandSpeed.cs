using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Localization;

namespace PartySizeReunited.HarmonyPatches.PartySpeed
{
    [HarmonyPatch(typeof(DefaultPartySpeedCalculatingModel), "CalculateLandBaseSpeed")]
    class Patch_PlayerPartyLandSpeed
    {
        public static void Postfix(MobileParty mobileParty, ref ExplainedNumber __result)
        {
            if (!SubModule.PartySizeReunitedOptions.IsActivate)
            {
                return;
            }
            if (mobileParty.Party.LeaderHero == null || !mobileParty.Party.LeaderHero.IsHumanPlayerCharacter)
            {
                return;
            }

            var options = SubModule.PartySizeReunitedOptions;
            TextObject description = new("Party Size Reunited modifier");
            if (options.PlayerPartySpeedFixedBonus != 0)
            {
                __result.Add(
                    options.PlayerPartySpeedFixedBonus,
                    description
                );
            }
            else
            {
                __result.AddFactor(
                    options.PlayerPartySpeedMultiBonus,
                    description
                    );
            }
        }
    }
}
