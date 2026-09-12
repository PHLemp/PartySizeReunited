namespace TaleWorlds.CampaignSystem
{
    public class Hero
    {
        public static Hero? MainHero { get; set; }
        public bool IsHumanPlayerCharacter { get; set; }
        public Clan? Clan { get; set; }
        public int Gold { get; set; }
    }

    public class Clan
    {
        public Kingdom? Kingdom { get; set; }
    }

    public class Kingdom
    {
        public string Name { get; set; } = string.Empty;
    }
}

namespace TaleWorlds.CampaignSystem.Party
{
    public class PartyBase
    {
        public Hero? Owner { get; set; }
        public Hero? LeaderHero { get; set; }
        public bool IsSettlement { get; set; }
        public Settlements.Settlement? Settlement { get; set; }
        public bool IsMobile { get; set; }
        public MobileParty MobileParty { get; set; } = new MobileParty();
    }

    public class MobileParty
    {
        public bool IsMainParty { get; set; }
        public Clan? ActualClan { get; set; }
        public bool IsCaravan { get; set; }
        public int TotalWage { get; set; }
    }
}

namespace TaleWorlds.CampaignSystem.Settlements
{
    public class Settlement
    {
        public Hero? Owner { get; set; }
    }
}

namespace TaleWorlds.CampaignSystem.GameComponents
{
    using TaleWorlds.CampaignSystem.Party;

    public class ExplainedNumber
    {
        public ExplainedNumber(float resultNumber)
        {
            ResultNumber = resultNumber;
        }

        public float ResultNumber { get; private set; }

        public void Add(float value, TaleWorlds.Localization.TextObject text)
        {
            ResultNumber += value;
        }
    }

    public class DefaultPartySizeLimitModel
    {
        public virtual ExplainedNumber GetPartyMemberSizeLimit(PartyBase party, bool includeDescriptions = false)
        {
            return new ExplainedNumber(0);
        }
    }
}

namespace TaleWorlds.Localization
{
    public class TextObject
    {
        public TextObject(string value)
        {
        }
    }
}

