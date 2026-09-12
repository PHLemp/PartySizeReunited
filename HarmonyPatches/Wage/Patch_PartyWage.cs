using HarmonyLib;
using PartySizeReunited.Models;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Localization;

namespace PartySizeReunited.HarmonyPatches.Wage
{
    [HarmonyPatch(typeof(DefaultPartyWageModel), "GetTotalWage")]
    internal class Patch_PartyWage
    {
        private static readonly TextObject partySizeBonusText = new TextObject("Party Size Reunited wage modifier");

        public static void Postfix(MobileParty mobileParty, ref ExplainedNumber __result)
        {
            if (!SubModule.PartySizeReunitedOptions.IsActivate || !SubModule.PartySizeReunitedOptions.IsWageActivate)
                return;

            if (mobileParty.IsGarrison)
                return;

            Scope selectedScope = SubModule.PartySizeReunitedOptions.BonusScope.SelectedValue.Scope;
            float bonusPercentage = SubModule.PartySizeReunitedOptions.PartyWageMultiBonus;
            int fixedBonus = SubModule.PartySizeReunitedOptions.PartyWageFixedBonus;
            float newValue = fixedBonus != 0 ?
                __result.ResultNumber + fixedBonus : // If fixedBonus is set
                (float)Math.Round(__result.ResultNumber * bonusPercentage); // If not, we take the multiplicator

            float valueToApply = newValue - __result.ResultNumber;

            if (ScopeExtension.IsInScope(selectedScope, mobileParty))
            {
                __result.Add(valueToApply, partySizeBonusText);
            }
        }
    }
}
