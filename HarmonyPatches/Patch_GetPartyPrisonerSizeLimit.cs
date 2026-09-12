using HarmonyLib;
using PartySizeReunited.Models;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Localization;

namespace PartySizeReunited.HarmonyPatches
{
    /// <summary>
    /// This patch try to add a custom party size mutliplier after the execution of "GetPartyPrisonerSizeLimit" that will define the default prisoner party size limit.
    /// </summary>
    [HarmonyPatch(typeof(DefaultPartySizeLimitModel), nameof(DefaultPartySizeLimitModel.GetPartyPrisonerSizeLimit))]
    class GetPartyPrisonerSizeLimit
    {
        static void Postfix(PartyBase party, ref ExplainedNumber __result)
        {
            if (!ShouldApplyPatch(party)) return;
            
            var options = SubModule.PartySizeReunitedOptions;
            TextObject description = new("Party Size Reunited modifier");
            if (options.PartyPrisonerFixedBonus != 0)
            {
                __result.Add(
                    options.PartyPrisonerFixedBonus,
                    description
                );
            }
            else
            {
                __result.AddFactor(
                    options.PartyPrisonerMultiBonus,
                    description
                );
            }
        }

        private static bool ShouldApplyPatch(PartyBase party)
        {
            var options = SubModule.PartySizeReunitedOptions;
            if (!options.IsActivate)
            {
                return false;
            }

            if (!options.IsPrisonerActivate)
            {
                return false;
            }

            return ScopeExtension.IsInScope(options.BonusScope.SelectedValue.Scope, party);
        }
    }
}
