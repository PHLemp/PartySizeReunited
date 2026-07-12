using MCM.Common;
using PartySizeReunited.Models;
using PartySizeReunited.Services;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Localization;

namespace PartySizeReunited
{
    public class PartySize : DefaultPartySizeLimitModel
    {
        private readonly List<DefaultPartySizeLimitModel> _partySizeModels;
        private static readonly TextObject partySizeBonusText = new TextObject("Party Size Reunited modifier");

        public PartySize(List<DefaultPartySizeLimitModel> partySizeModels)
        {
            _partySizeModels = partySizeModels ?? new List<DefaultPartySizeLimitModel>();
        }

        public override ExplainedNumber GetPartyMemberSizeLimit(PartyBase party, bool includeDescriptions = false)
        {
            ExplainedNumber partySize = base.GetPartyMemberSizeLimit(party, includeDescriptions);

            // Appliquer tous les modèles de taille découverts
            foreach (var model in _partySizeModels)
            {
                try
                {
                    partySize = model.GetPartyMemberSizeLimit(party, includeDescriptions);
                }
                catch (Exception ex)
                {
                    Utils.PrintError($"Error applying model {model.GetType().Name}: {ex.Message}");
                }
            }

            return SubModule.PartySizeReunitedOptions.IsActivate ? GetPartySizeUpdatedByPartySizeMod(party, partySize) : partySize;
        }

        private ExplainedNumber GetPartySizeUpdatedByPartySizeMod(PartyBase party, ExplainedNumber basePartySize)
        {
            Dropdown<ScopeExtension> bonusScope = SubModule.PartySizeReunitedOptions.BonusScope;
            IScope selectedScope = bonusScope.SelectedValue.Scope;
            float bonusPercentage = SubModule.PartySizeReunitedOptions.PartyBonusAmnt;
            int fixedBonus = SubModule.PartySizeReunitedOptions.FixedBonusAmnt;
            bool isPlayerImpacted = SubModule.PartySizeReunitedOptions.IsPlayerPartyImpacted;

            ExplainedNumber result = basePartySize;
            float newValue = fixedBonus != 0 ?
                result.ResultNumber + fixedBonus : // If fixedBonus is set
                (float)Math.Round(result.ResultNumber * bonusPercentage); // If not, we take the multiplicator

            float valueToApply = newValue - result.ResultNumber;

            if (!IsCaravan(party))
            {
                switch (selectedScope)
                {
                    case IScope.Everyone:
                        if (ScopeExtension.IsEveryoneExceptPlayer(party))
                        {
                            result.Add(valueToApply, partySizeBonusText);
                        }
                        break;

                    case IScope.Only_player_clan:
                        if (ScopeExtension.IsOnlyPlayerClan(party))
                        {
                            result.Add(valueToApply, partySizeBonusText);
                        }
                        break;

                    case IScope.Only_player_kingdom:
                        if (ScopeExtension.IsOnlyPlayerKingdom(party))
                        {
                            result.Add(valueToApply, partySizeBonusText);
                        }
                        break;

                    case IScope.Only_ennemies:
                        if (ScopeExtension.IsOnlyEnnemies(party))
                        {
                            result.Add(valueToApply, partySizeBonusText);
                        }
                        break;
                }

                SetNoMoreSupplyNeeded(party);

                if (isPlayerImpacted && party.Owner != null && party.Owner.IsHumanPlayerCharacter)
                {
                    // Update player's party
                    result.Add(valueToApply, partySizeBonusText);
                }
            }
            result = HandleCaravans(party, result);
            result = HandleGarrisons(party, result);

            return result;
        }

        private ExplainedNumber HandleGarrisons(PartyBase party, ExplainedNumber result)
        {
            if (!SubModule.PartySizeReunitedOptions.IsGarrisonActivate)
                return result;

            if (!party.IsSettlement || party.Settlement == null)
                return result;

            float bonusPercentage = SubModule.PartySizeReunitedOptions.CaravanMultiBonus;
            int fixedBonus = SubModule.PartySizeReunitedOptions.CaravanFixedBonus;

            float newValue = fixedBonus != 0 ?
                result.ResultNumber + fixedBonus : // If fixedBonus is set
                (float)Math.Round(result.ResultNumber * bonusPercentage); // If not, we take the multiplicator

            float valueToApply = newValue - result.ResultNumber;


            result.Add(valueToApply, partySizeBonusText);

            return result;
        }

        private ExplainedNumber HandleCaravans(PartyBase party, ExplainedNumber result)
        {
            if (!SubModule.PartySizeReunitedOptions.IsCaravanActivate)
                return result;

            if (!IsCaravan(party))
                return result;

            float bonusPercentage = SubModule.PartySizeReunitedOptions.CaravanMultiBonus;
            int fixedBonus = SubModule.PartySizeReunitedOptions.CaravanFixedBonus;

            float newValue = fixedBonus != 0 ?
                result.ResultNumber + fixedBonus : // If fixedBonus is set
                (float)Math.Round(result.ResultNumber * bonusPercentage); // If not, we take the multiplicator

            float valueToApply = newValue - result.ResultNumber;


            result.Add(valueToApply, partySizeBonusText);

            return result;
        }

        private void SetNoMoreSupplyNeeded(PartyBase party)
        {
            bool noMoreSupplyNeeded = SubModule.PartySizeReunitedOptions.NoMoreSupplyIssues;
            if (noMoreSupplyNeeded)
            {
                SetGoldBonus(party);
            }
        }

        private void SetGoldBonus(PartyBase party)
        {
            if (party.LeaderHero != null && party.LeaderHero.Gold < party.MobileParty.TotalWage)
            {
                int bonus = party.MobileParty.TotalWage * 2;
                party.LeaderHero.Gold += bonus;
            }
        }

        private bool IsCaravan(PartyBase party)
        {
            return party != null && party.IsMobile && party.MobileParty.IsCaravan;
        }
    }
}
