using HarmonyLib;
using MCM.Common;
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

            Dropdown<ScopeExtension> bonusScope = SubModule.PartySizeReunitedOptions.BonusScope;
            IScope selectedScope = bonusScope.SelectedValue.Scope;
            float bonusPercentage = SubModule.PartySizeReunitedOptions.PartyWageMultiBonus;
            int fixedBonus = SubModule.PartySizeReunitedOptions.PartyWageFixedBonus;
            bool isPlayerImpacted = SubModule.PartySizeReunitedOptions.IsPlayerPartyImpacted;

            float newValue = fixedBonus != 0 ?
                __result.ResultNumber + fixedBonus : // If fixedBonus is set
                (float)Math.Round(__result.ResultNumber * bonusPercentage); // If not, we take the multiplicator

            float valueToApply = newValue - __result.ResultNumber;

            switch (selectedScope)
            {
                case IScope.Everyone:
                    if (ScopeExtension.IsEveryoneExceptPlayer(mobileParty))
                    {
                        __result.Add(valueToApply, partySizeBonusText);
                    }
                    break;

                case IScope.Only_player_clan:
                    if (ScopeExtension.IsOnlyPlayerClan(mobileParty))
                    {
                        __result.Add(valueToApply, partySizeBonusText);
                    }
                    break;

                case IScope.Only_player_kingdom:
                    if (ScopeExtension.IsOnlyPlayerKingdom(mobileParty))
                    {
                        __result.Add(valueToApply, partySizeBonusText);
                    }
                    break;

                case IScope.Only_ennemies:
                    if (ScopeExtension.IsOnlyEnnemies(mobileParty))
                    {
                        __result.Add(valueToApply, partySizeBonusText);
                    }
                    break;
            }

            if (isPlayerImpacted && mobileParty.LeaderHero?.IsHumanPlayerCharacter == true)
            {
                // Update player's party
                __result.Add(valueToApply, partySizeBonusText);
            }
        }
    }
}
