using HarmonyLib;
using PartySizeReunited.Models;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace PartySizeReunited.HarmonyPatches
{
    /// <summary>
    /// This patch try to add a custom party size mutliplier after the execution of "CalculateGarrisonPartySizeLimit" that will define the default party size limit of a garrison.
    /// This limit may vary due to other calculation such as food supply or prosperity.
    /// </summary>
    [HarmonyPatch(typeof(DefaultPartySizeLimitModel), nameof(DefaultPartySizeLimitModel.CalculateGarrisonPartySizeLimit))]
    class Patch_GarrisonPartySizeLimit
    {
        static void Postfix(Settlement settlement, ref ExplainedNumber __result)
        {
            if (ShouldApplyPatch(settlement))
            {
                var options = SubModule.PartySizeReunitedOptions;
                TextObject description = new("PartySizeReunited modifier");
                if (options.PartyGarrisonFixedBonus != 0)
                {
                    __result.Add(
                        options.PartyGarrisonFixedBonus,
                        description
                    );
                }
                else
                {
                    __result.AddFactor(
                        options.PartyGarrisonMultiBonus,
                        description
                        );
                }
            }
        }

        private static bool ShouldApplyPatch(Settlement settlement)
        {
            var options = SubModule.PartySizeReunitedOptions;
            if (!options.IsActivate)
            {
                return false;
            }

            if (!options.IsGarrisonActivate)
            {
                return false;
            }

            return options.BonusScope.SelectedValue.Scope switch
            {
                IScope.Everyone => true,
                IScope.Only_player => options.IsPlayerPartyImpacted && settlement.Owner != null && settlement.Owner.IsHumanPlayerCharacter,
                IScope.Only_player_clan => ScopeExtension.IsOnlyPlayerClan(settlement.OwnerClan),
                IScope.Only_player_kingdom => ScopeExtension.IsOnlyPlayerKingdom(settlement.OwnerClan.Kingdom),
                IScope.Only_ennemies => ScopeExtension.IsOnlyEnnemies(settlement.OwnerClan.Kingdom),
                _ => false,
            };
        }
    }
}
