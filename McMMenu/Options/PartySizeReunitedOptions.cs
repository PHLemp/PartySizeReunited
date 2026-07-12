using MCM.Common;
using PartySizeReunited.Models;

namespace PartySizeReunited.McMMenu.Options
{
    public class PartySizeReunitedOptions
    {
        public bool IsActivate { get; set; } = true;
        public bool IsWageActivate { get; set; }
        public bool IsPrisonerActivate { get; set; }
        public bool IsGarrisonActivate { get; set; }
        public bool IsCaravanActivate { get; set; }
        public bool IsPlayerPartyImpacted { get; set; }
        public bool NoMoreSupplyIssues { get; set; }
        public float PartyBonusAmnt { get; set; } = 1f;
        public float PartyInfluenceCost { get; set; } = 1f;
        public int FixedBonusAmnt { get; set; } = 0;
        // Caravan
        public int CaravanFixedBonus { get; set; } = 0;
        public float CaravanMultiBonus { get; set; } = 1f;
        // Wage
        public int PartyWageFixedBonus { get; set; } = 0;
        public float PartyWageMultiBonus { get; set; } = 1f;
        // Player speed
        public int PlayerPartySpeedFixedBonus { get; set; } = 0;
        public float PlayerPartySpeedMultiBonus { get; set; } = 1f;
        // Garrison
        public int PartyGarrisonFixedBonus { get; set; } = 0;
        public float PartyGarrisonMultiBonus { get; set; } = 1f;
        // Prisoner
        public int PartyPrisonerFixedBonus { get; set; } = 0;
        public float PartyPrisonerMultiBonus { get; set; } = 1f;

        public Dropdown<ScopeExtension> BonusScope { get; set; } = new Dropdown<ScopeExtension>(new ScopeExtension[]
        {
            new (IScope.Everyone),
            new (IScope.Only_player),
            new (IScope.Only_player_clan),
            new (IScope.Only_player_kingdom),
            new (IScope.Only_ennemies)
        }, 0);
    }
}
