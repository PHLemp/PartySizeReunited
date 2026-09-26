using PartySizeReunited.HarmonyPatches;
using PartySizeReunited.McMMenu.Options;
using PartySizeReunited.Tests.TestDoubles;
using Xunit;

namespace PartySizeReunited.Tests.Tests
{
    public class NavalShipLimitPatchTests
    {
        [Theory]
        [InlineData(50, 3)]
        [InlineData(300, 7)]
        [InlineData(3000, 67)]
        public void Postfix_ScalesLordTargetFromSmallToVeryLargeParties(int troopCount, int expected)
        {
            SubModule.WarSailsOptions = EnabledOptions();
            FakeParty party = CreateParty(troopCount, isLordParty: true);
            var result = 3;

            Patch_NavalDLC_ShipLimitModel.Postfix(party, ref result);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(10000)]
        [InlineData(500000)]
        public void Postfix_KeepsCaravanTargetIndependentFromCarriedCargo(float carriedWeight)
        {
            // La capacité d'inventaire d'une caravane augmente avec ses navires : si
            // la cible dépendait du poids transporté, chaque achat en justifierait un
            // nouveau et la flotte grossirait indéfiniment.
            SubModule.WarSailsOptions = EnabledOptions();
            FakeParty party = CreateParty(50, isCaravan: true);
            party.TotalWeightCarried = carriedWeight;
            var result = 3;

            Patch_NavalDLC_ShipLimitModel.Postfix(party, ref result);

            Assert.Equal(3, result);
        }

        [Fact]
        public void Postfix_CaravanTargetStaysStableWhileFleetAndCargoGrow()
        {
            SubModule.WarSailsOptions = EnabledOptions();
            FakeParty party = CreateParty(60, isCaravan: true);

            for (int purchased = 0; purchased < 10; purchased++)
            {
                party.Ships.Add(new FakeShip
                {
                    ShipHull = new FakeShipHull { Type = 0, TotalCrewCapacity = 32, InventoryCapacity = 8000 }
                });
                // Chaque navire permet d'emporter davantage de marchandises.
                party.TotalWeightCarried += 7200f;

                var result = 3;
                Patch_NavalDLC_ShipLimitModel.Postfix(party, ref result);

                Assert.Equal(3, result);
            }
        }

        [Fact]
        public void Postfix_DoesNotGivePiratesTheCaravanReserve()
        {
            SubModule.WarSailsOptions = EnabledOptions();
            FakeParty party = CreateParty(300, isBandit: true);
            var result = 3;

            Patch_NavalDLC_ShipLimitModel.Postfix(party, ref result);

            Assert.Equal(7, result);
        }

        [Theory]
        // northern_pirates_template : 2 coques légères (32) + 1 moyenne (54).
        [InlineData(65, 32, 54)]
        // southern_pirates_template : 2 coques légères (30) + 1 moyenne (60).
        [InlineData(65, 30, 60)]
        public void Postfix_KeepsNativeTargetForStandardPirateParties(int troopCount, int lightCapacity,
            int mediumCapacity)
        {
            SubModule.WarSailsOptions = EnabledOptions();
            FakeParty party = CreateParty(troopCount, isBandit: true);
            // Les cultures bandit ne déclarent aucune coque : seule la flotte compte.
            party.MapFaction.Culture.AvailableShipHulls.Clear();
            party.Ships.Add(new FakeShip { ShipHull = new FakeShipHull { TotalCrewCapacity = lightCapacity } });
            party.Ships.Add(new FakeShip { ShipHull = new FakeShipHull { TotalCrewCapacity = lightCapacity } });
            party.Ships.Add(new FakeShip { ShipHull = new FakeShipHull { TotalCrewCapacity = mediumCapacity } });
            var result = 3;

            Patch_NavalDLC_ShipLimitModel.Postfix(party, ref result);

            Assert.Equal(3, result);
        }

        [Fact]
        public void Postfix_NeverLetsPirateTargetFallBelowNativeMinimum()
        {
            SubModule.WarSailsOptions = EnabledOptions();
            FakeParty party = CreateParty(20, isBandit: true);
            party.MapFaction.Culture.AvailableShipHulls.Clear();
            party.Ships.Add(new FakeShip { ShipHull = new FakeShipHull { TotalCrewCapacity = 106 } });
            var result = 3;

            Patch_NavalDLC_ShipLimitModel.Postfix(party, ref result);

            Assert.Equal(3, result);
        }

        [Fact]
        public void Postfix_ScalesPirateTargetWhenPartySizeIsIncreased()
        {
            SubModule.WarSailsOptions = EnabledOptions();
            FakeParty party = CreateParty(300, isBandit: true);
            party.MapFaction.Culture.AvailableShipHulls.Clear();
            party.Ships.Add(new FakeShip { ShipHull = new FakeShipHull { TotalCrewCapacity = 32 } });
            party.Ships.Add(new FakeShip { ShipHull = new FakeShipHull { TotalCrewCapacity = 32 } });
            party.Ships.Add(new FakeShip { ShipHull = new FakeShipHull { TotalCrewCapacity = 54 } });
            var result = 3;

            Patch_NavalDLC_ShipLimitModel.Postfix(party, ref result);

            Assert.Equal(9, result);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void Postfix_IgnoresHeavyHullsUnavailableToCaravansAndPirates(bool isCaravan, bool isBandit)
        {
            SubModule.WarSailsOptions = EnabledOptions();
            FakeParty party = CreateParty(300, isCaravan: isCaravan, isBandit: isBandit);
            party.MapFaction.Culture.AvailableShipHulls = new()
            {
                new FakeShipHull { Type = 0, TotalCrewCapacity = 30, InventoryCapacity = 2000 },
                new FakeShipHull { Type = 2, TotalCrewCapacity = 300, InventoryCapacity = 10000 }
            };
            var result = 3;

            Patch_NavalDLC_ShipLimitModel.Postfix(party, ref result);

            Assert.Equal(11, result);
        }

        [Fact]
        public void Postfix_UsesActualFleetCapacityWhenOwnedShipsAreSmallerThanCulturalMedian()
        {
            SubModule.WarSailsOptions = EnabledOptions();
            FakeParty party = CreateParty(700, isLordParty: true);
            party.MapFaction.Culture.AvailableShipHulls = new()
            {
                new FakeShipHull { TotalCrewCapacity = 30 },
                new FakeShipHull { TotalCrewCapacity = 35 },
                new FakeShipHull { TotalCrewCapacity = 40 }
            };
            for (int i = 0; i < 17; i++)
            {
                party.Ships.Add(new FakeShip
                {
                    ShipHull = new FakeShipHull { TotalCrewCapacity = 18 }
                });
            }
            var result = 3;

            Patch_NavalDLC_ShipLimitModel.Postfix(party, ref result);

            Assert.Equal(43, result);
        }

        [Fact]
        public void Postfix_UsesConservativeCapacityWhenCultureDeclaresNoShipHull()
        {
            // Les données du jeu laissent <available_ship_hulls> vide : sans navire
            // possédé, la cible doit rester prudente au lieu de supposer de grandes coques.
            SubModule.WarSailsOptions = EnabledOptions();
            FakeParty party = CreateParty(978, isLordParty: true);
            party.MapFaction.Culture.AvailableShipHulls.Clear();
            var result = 3;

            Patch_NavalDLC_ShipLimitModel.Postfix(party, ref result);

            Assert.Equal(27, result);
        }

        [Fact]
        public void Postfix_CoversVlandianLordOfNineHundredSeventyEightTroops()
        {
            // Coques vlandiennes réelles (XSLT NavalDLC_SandBoxCore_SPCultures) :
            // médiane 50, donc 20 navires ne couvraient que 1000 places sans marge.
            SubModule.WarSailsOptions = EnabledOptions();
            FakeParty party = CreateParty(978, isLordParty: true);
            party.MapFaction.Culture.AvailableShipHulls = new()
            {
                new FakeShipHull { Type = 0, TotalCrewCapacity = 32 },
                new FakeShipHull { Type = 1, TotalCrewCapacity = 50 },
                new FakeShipHull { Type = 1, TotalCrewCapacity = 60 },
                new FakeShipHull { Type = 1, TotalCrewCapacity = 30 },
                new FakeShipHull { Type = 2, TotalCrewCapacity = 108 }
            };
            var result = 3;

            Patch_NavalDLC_ShipLimitModel.Postfix(party, ref result);

            Assert.Equal(22, result);
        }

        [Fact]
        public void Postfix_PrefersOwnedFleetCapacityOverCulturalHulls()
        {
            SubModule.WarSailsOptions = EnabledOptions();
            FakeParty party = CreateParty(1000, isLordParty: true);
            party.MapFaction.Culture.AvailableShipHulls = new()
            {
                new FakeShipHull { TotalCrewCapacity = 40 }
            };
            for (int i = 0; i < 4; i++)
            {
                party.Ships.Add(new FakeShip
                {
                    ShipHull = new FakeShipHull { TotalCrewCapacity = 100 }
                });
            }
            var result = 3;

            Patch_NavalDLC_ShipLimitModel.Postfix(party, ref result);

            Assert.Equal(11, result);
        }

        [Fact]
        public void Postfix_SupportsThreeThousandTroopsWithSmallNativeTradeShips()
        {
            SubModule.WarSailsOptions = EnabledOptions();
            FakeParty party = CreateParty(3000, isCaravan: true);
            party.Ships.Add(new FakeShip
            {
                ShipHull = new FakeShipHull { Type = 0, TotalCrewCapacity = 16 }
            });
            var result = 3;

            Patch_NavalDLC_ShipLimitModel.Postfix(party, ref result);

            Assert.Equal(207, result);
        }

        [Fact]
        public void Postfix_ClampsVeryLargePartyToConfiguredMaximum()
        {
            WarSailsOptions options = EnabledOptions();
            options.MaximumIdealShips = 20;
            SubModule.WarSailsOptions = options;
            FakeParty party = CreateParty(3000, isLordParty: true);
            var result = 3;

            Patch_NavalDLC_ShipLimitModel.Postfix(party, ref result);

            Assert.Equal(20, result);
        }

        [Fact]
        public void Postfix_DoesNotChangeUnhandledPartyType()
        {
            SubModule.WarSailsOptions = EnabledOptions();
            var result = 3;

            Patch_NavalDLC_ShipLimitModel.Postfix(CreateParty(3000), ref result);

            Assert.Equal(3, result);
        }

        [Fact]
        public void Postfix_DoesNotChangeResultWhenDisabled()
        {
            SubModule.WarSailsOptions = new WarSailsOptions { IsActivate = false };
            var result = 3;

            Patch_NavalDLC_ShipLimitModel.Postfix(CreateParty(3000, isLordParty: true), ref result);

            Assert.Equal(3, result);
        }

        [Fact]
        public void Postfix_DoesNotChangePlayerParty()
        {
            SubModule.WarSailsOptions = EnabledOptions();
            FakeParty party = CreateParty(3000, isLordParty: true);
            party.LeaderHero = new FakeHero { IsHumanPlayerCharacter = true };
            var result = 3;

            Patch_NavalDLC_ShipLimitModel.Postfix(party, ref result);

            Assert.Equal(3, result);
        }

        [Fact]
        public void Postfix_LeavesResultUnchangedWhenPartyIsNull()
        {
            SubModule.WarSailsOptions = EnabledOptions();
            var result = 3;

            var exception = Record.Exception(() =>
                Patch_NavalDLC_ShipLimitModel.Postfix(null!, ref result));

            Assert.Null(exception);
            Assert.Equal(3, result);
        }

        [Fact]
        public void ClanPostfix_SumsDynamicTargetsForEveryWarParty()
        {
            SubModule.WarSailsOptions = EnabledOptions();
            FakeClan.PlayerClan = null;
            var clan = new FakeClan
            {
                WarPartyComponents =
                {
                    new FakeWarPartyComponent { MobileParty = CreateParty(50, isLordParty: true) },
                    new FakeWarPartyComponent { MobileParty = CreateParty(3000, isLordParty: true) }
                }
            };
            var result = 6;

            Patch_NavalDLC_ShipLimitModel.ClanPostfix(clan, ref result);

            Assert.Equal(70, result);
        }

        [Fact]
        public void ClanPostfix_DoesNotChangePlayerClan()
        {
            SubModule.WarSailsOptions = EnabledOptions();
            var clan = new FakeClan
            {
                WarPartyComponents =
                {
                    new FakeWarPartyComponent { MobileParty = CreateParty(3000, isLordParty: true) }
                }
            };
            FakeClan.PlayerClan = clan;
            var result = 3;

            try
            {
                Patch_NavalDLC_ShipLimitModel.ClanPostfix(clan, ref result);
                Assert.Equal(3, result);
            }
            finally
            {
                FakeClan.PlayerClan = null;
            }
        }

        private static WarSailsOptions EnabledOptions()
            => new WarSailsOptions
            {
                IsActivate = true,
                MaximumIdealShips = 250
            };

        private static FakeParty CreateParty(int troopCount, bool isCaravan = false,
            bool isLordParty = false, bool isBandit = false)
            => new FakeParty
            {
                IsCaravan = isCaravan,
                IsLordParty = isLordParty,
                IsBandit = isBandit,
                Party = new FakePartyBase
                {
                    MemberRoster = new FakeMemberRoster { TotalManCount = troopCount }
                },
                MapFaction = new FakeMapFaction
                {
                    Culture = new FakeCulture
                    {
                        AvailableShipHulls =
                        {
                            new FakeShipHull { TotalCrewCapacity = 40, InventoryCapacity = 2000 },
                            new FakeShipHull { TotalCrewCapacity = 50, InventoryCapacity = 3000 },
                            new FakeShipHull { TotalCrewCapacity = 60, InventoryCapacity = 4000 }
                        }
                    }
                }
            };
    }
}

