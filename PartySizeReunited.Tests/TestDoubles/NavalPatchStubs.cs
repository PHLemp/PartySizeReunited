namespace PartySizeReunited.Tests.TestDoubles
{
    public class FakeHero
    {
        public bool IsHumanPlayerCharacter { get; set; }
    }

    public class FakeParty
    {
        public FakeHero? LeaderHero { get; set; }
        public bool IsMainParty { get; set; }
        public bool IsCaravan { get; set; }
        public bool IsLordParty { get; set; }
        public bool IsBandit { get; set; }
        public float TotalWeightCarried { get; set; }
        public FakePartyBase Party { get; set; } = new FakePartyBase();
        public FakeMapFaction MapFaction { get; set; } = new FakeMapFaction();
        public List<FakeShip> Ships { get; set; } = new List<FakeShip>();
    }

    public class FakePartyBase
    {
        public FakeMemberRoster MemberRoster { get; set; } = new FakeMemberRoster();
    }

    public class FakeMemberRoster
    {
        public int TotalManCount { get; set; }
    }

    public class FakeMapFaction
    {
        public FakeCulture Culture { get; set; } = new FakeCulture();
    }

    public class FakeCulture
    {
        public List<FakeShipHull> AvailableShipHulls { get; set; } = new List<FakeShipHull>();
    }

    public class FakeShipHull
    {
        public int Type { get; set; }
        public int TotalCrewCapacity { get; set; }
        public int InventoryCapacity { get; set; }
    }

    public class FakeShip
    {
        public FakeShipHull ShipHull { get; set; } = new FakeShipHull();
    }

    public class FakeClan
    {
        public static FakeClan? PlayerClan { get; set; }
        public FakeHero? Leader { get; set; }
        public List<FakeWarPartyComponent> WarPartyComponents { get; set; } = new List<FakeWarPartyComponent>();
    }

    public class FakeWarPartyComponent
    {
        public FakeParty MobileParty { get; set; } = new FakeParty();
    }
}
