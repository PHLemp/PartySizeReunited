using PartySizeReunited.Models;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using Xunit;

namespace PartySizeReunited.Tests.Tests
{
    public class ScopeExtensionComprehensiveTests
    {
        [Fact]
        public void PartyBase_IsOnlyPlayer_ReturnsExpectedValues()
        {
            var playerClan = new Clan { Kingdom = new Kingdom { Name = "Vlandia" } };
            var playerParty = new PartyBase { Owner = new Hero { IsHumanPlayerCharacter = true, Clan = playerClan } };
            var aiParty = new PartyBase { Owner = new Hero { IsHumanPlayerCharacter = false, Clan = playerClan } };
            var previousOptions = SubModule.PartySizeReunitedOptions;

            try
            {
                SubModule.PartySizeReunitedOptions = new McMMenu.Options.PartySizeReunitedOptions
                {
                    IsPlayerPartyImpacted = false
                };
                Assert.False(ScopeExtension.IsInScope(Scope.OnlyPlayer, playerParty));

                SubModule.PartySizeReunitedOptions.IsPlayerPartyImpacted = true;
                Assert.True(ScopeExtension.IsInScope(Scope.OnlyPlayer, playerParty));
                Assert.False(ScopeExtension.IsInScope(Scope.OnlyPlayer, aiParty));
                Assert.False(ScopeExtension.IsInScope(Scope.OnlyPlayer, (PartyBase)null!));
            }
            finally
            {
                SubModule.PartySizeReunitedOptions = previousOptions;
            }
        }

        [Fact]
        public void PartyBase_IsEveryoneExceptPlayer_ReturnsExpectedValues()
        {
            var playerParty = new PartyBase { Owner = new Hero { IsHumanPlayerCharacter = true } };
            var aiParty = new PartyBase { Owner = new Hero { IsHumanPlayerCharacter = false } };

            Assert.False(ScopeExtension.IsInScope(Scope.Everyone, playerParty));
            Assert.True(ScopeExtension.IsInScope(Scope.Everyone, aiParty));
            Assert.False(ScopeExtension.IsInScope(Scope.Everyone, (PartyBase)null!));
        }

        [Fact]
        public void PartyBase_IsOnlyPlayerClan_ReturnsExpectedValues()
        {
            var playerClan = new Clan { Kingdom = new Kingdom { Name = "Vlandia" } };
            Hero.MainHero = new Hero { Clan = playerClan };

            var sameClanAiParty = new PartyBase { Owner = new Hero { IsHumanPlayerCharacter = false, Clan = playerClan } };
            var differentClanAiParty = new PartyBase
            {
                Owner = new Hero { IsHumanPlayerCharacter = false, Clan = new Clan { Kingdom = playerClan.Kingdom } }
            };
            var playerParty = new PartyBase { Owner = new Hero { IsHumanPlayerCharacter = true, Clan = playerClan } };

            Assert.True(ScopeExtension.IsInScope(Scope.OnlyPlayerClan, sameClanAiParty));
            Assert.False(ScopeExtension.IsInScope(Scope.OnlyPlayerClan, differentClanAiParty));
            Assert.False(ScopeExtension.IsInScope(Scope.OnlyPlayerClan, playerParty));
            Assert.False(ScopeExtension.IsInScope(Scope.OnlyPlayerClan, (PartyBase)null!));
        }

        [Fact]
        public void PartyBase_IsOnlyPlayerKingdom_ReturnsExpectedValues()
        {
            var playerKingdom = new Kingdom { Name = "Vlandia" };
            var playerClan = new Clan { Kingdom = playerKingdom };
            Hero.MainHero = new Hero { Clan = playerClan };

            var sameKingdomAiParty = new PartyBase
            {
                Owner = new Hero { IsHumanPlayerCharacter = false, Clan = new Clan { Kingdom = new Kingdom { Name = "Vlandia" } } }
            };
            var differentKingdomAiParty = new PartyBase
            {
                Owner = new Hero { IsHumanPlayerCharacter = false, Clan = new Clan { Kingdom = new Kingdom { Name = "Aserai" } } }
            };

            Assert.True(ScopeExtension.IsInScope(Scope.OnlyPlayerKingdom, sameKingdomAiParty));
            Assert.False(ScopeExtension.IsInScope(Scope.OnlyPlayerKingdom, differentKingdomAiParty));
            Assert.False(ScopeExtension.IsInScope(Scope.OnlyPlayerKingdom, (PartyBase)null!));
        }

        [Fact]
        public void PartyBase_IsOnlyEnnemies_ReturnsExpectedValues()
        {
            var playerKingdom = new Kingdom { Name = "Vlandia" };
            var playerClan = new Clan { Kingdom = playerKingdom };
            Hero.MainHero = new Hero { Clan = playerClan };

            var sameClanAiParty = new PartyBase { Owner = new Hero { IsHumanPlayerCharacter = false, Clan = playerClan } };
            var sameKingdomDifferentClanAiParty = new PartyBase
            {
                Owner = new Hero { IsHumanPlayerCharacter = false, Clan = new Clan { Kingdom = new Kingdom { Name = "Vlandia" } } }
            };
            var differentKingdomAiParty = new PartyBase
            {
                Owner = new Hero { IsHumanPlayerCharacter = false, Clan = new Clan { Kingdom = new Kingdom { Name = "Aserai" } } }
            };
            var playerParty = new PartyBase { Owner = new Hero { IsHumanPlayerCharacter = true, Clan = playerClan } };

            Assert.False(ScopeExtension.IsInScope(Scope.OnlyEnnemies, sameClanAiParty));
            Assert.False(ScopeExtension.IsInScope(Scope.OnlyEnnemies, sameKingdomDifferentClanAiParty));
            Assert.True(ScopeExtension.IsInScope(Scope.OnlyEnnemies, differentKingdomAiParty));
            Assert.False(ScopeExtension.IsInScope(Scope.OnlyEnnemies, playerParty));
            Assert.False(ScopeExtension.IsInScope(Scope.OnlyEnnemies, (PartyBase)null!));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void PlayerOwnedEntities_AreIncludedInEveryScopeOnlyWhenPlayerImpactIsEnabled(bool isPlayerPartyImpacted)
        {
            var playerKingdom = new Kingdom { Name = "Vlandia" };
            var playerClan = new Clan { Kingdom = playerKingdom };
            var previousOptions = SubModule.PartySizeReunitedOptions;
            var previousMainHero = Hero.MainHero;
            Hero.MainHero = new Hero { IsHumanPlayerCharacter = true, Clan = playerClan };
            SubModule.PartySizeReunitedOptions = new McMMenu.Options.PartySizeReunitedOptions
            {
                IsPlayerPartyImpacted = isPlayerPartyImpacted
            };

            try
            {
                var playerParty = new PartyBase { Owner = new Hero { IsHumanPlayerCharacter = true, Clan = playerClan } };
                var playerMobileParty = new MobileParty { IsMainParty = true, ActualClan = playerClan };
                var playerSettlement = new Settlement { Owner = new Hero { IsHumanPlayerCharacter = true, Clan = playerClan } };

                foreach (Scope scope in Enum.GetValues<Scope>())
                {
                    Assert.Equal(isPlayerPartyImpacted, ScopeExtension.IsInScope(scope, playerParty));
                    Assert.Equal(isPlayerPartyImpacted, ScopeExtension.IsInScope(scope, playerMobileParty));
                    Assert.Equal(isPlayerPartyImpacted, ScopeExtension.IsInScope(scope, playerSettlement));
                }
            }
            finally
            {
                SubModule.PartySizeReunitedOptions = previousOptions;
                Hero.MainHero = previousMainHero;
            }
        }

        [Fact]
        public void MobileParty_IsOnlyPlayer_And_EveryoneExceptPlayer_ReturnExpectedValues()
        {
            var mainParty = new MobileParty { IsMainParty = true };
            var aiParty = new MobileParty { IsMainParty = false };
            var previousOptions = SubModule.PartySizeReunitedOptions;

            try
            {
                SubModule.PartySizeReunitedOptions = new McMMenu.Options.PartySizeReunitedOptions();
                Assert.False(ScopeExtension.IsInScope(Scope.OnlyPlayer, mainParty));
                Assert.False(ScopeExtension.IsInScope(Scope.Everyone, mainParty));

                SubModule.PartySizeReunitedOptions.IsPlayerPartyImpacted = true;
                Assert.True(ScopeExtension.IsInScope(Scope.OnlyPlayer, mainParty));
                Assert.False(ScopeExtension.IsInScope(Scope.OnlyPlayer, aiParty));
                Assert.False(ScopeExtension.IsInScope(Scope.OnlyPlayer, (MobileParty)null!));

                Assert.True(ScopeExtension.IsInScope(Scope.Everyone, mainParty));
                Assert.True(ScopeExtension.IsInScope(Scope.Everyone, aiParty));
                Assert.False(ScopeExtension.IsInScope(Scope.Everyone, (MobileParty)null!));
            }
            finally
            {
                SubModule.PartySizeReunitedOptions = previousOptions;
            }
        }

        [Fact]
        public void MobileParty_IsOnlyPlayerClan_And_IsOnlyPlayerKingdom_ReturnExpectedValues()
        {
            var playerKingdom = new Kingdom { Name = "Vlandia" };
            var playerClan = new Clan { Kingdom = playerKingdom };
            Hero.MainHero = new Hero { Clan = playerClan };

            var sameClanParty = new MobileParty { IsMainParty = false, ActualClan = playerClan };
            var sameKingdomDifferentClanParty = new MobileParty
            {
                IsMainParty = false,
                ActualClan = new Clan { Kingdom = new Kingdom { Name = "Vlandia" } }
            };
            var differentKingdomParty = new MobileParty
            {
                IsMainParty = false,
                ActualClan = new Clan { Kingdom = new Kingdom { Name = "Aserai" } }
            };

            Assert.True(ScopeExtension.IsInScope(Scope.OnlyPlayerClan, sameClanParty));
            Assert.False(ScopeExtension.IsInScope(Scope.OnlyPlayerClan, sameKingdomDifferentClanParty));
            Assert.False(ScopeExtension.IsInScope(Scope.OnlyPlayerClan, (MobileParty)null!));

            Assert.True(ScopeExtension.IsInScope(Scope.OnlyPlayerKingdom, sameClanParty));
            Assert.True(ScopeExtension.IsInScope(Scope.OnlyPlayerKingdom, sameKingdomDifferentClanParty));
            Assert.False(ScopeExtension.IsInScope(Scope.OnlyPlayerKingdom, differentKingdomParty));
            Assert.False(ScopeExtension.IsInScope(Scope.OnlyPlayerKingdom, (MobileParty)null!));
        }

        [Fact]
        public void MobileParty_IsOnlyEnnemies_ReturnsExpectedValues()
        {
            var playerKingdom = new Kingdom { Name = "Vlandia" };
            var playerClan = new Clan { Kingdom = playerKingdom };
            Hero.MainHero = new Hero { Clan = playerClan };

            var mainParty = new MobileParty { IsMainParty = true, ActualClan = playerClan };
            var nullKingdomParty = new MobileParty { IsMainParty = false, ActualClan = new Clan { Kingdom = null } };
            var sameKingdomParty = new MobileParty
            {
                IsMainParty = false,
                ActualClan = new Clan { Kingdom = new Kingdom { Name = "Vlandia" } }
            };
            var differentKingdomParty = new MobileParty
            {
                IsMainParty = false,
                ActualClan = new Clan { Kingdom = new Kingdom { Name = "Aserai" } }
            };

            Assert.False(ScopeExtension.IsInScope(Scope.OnlyEnnemies, mainParty));
            Assert.True(ScopeExtension.IsInScope(Scope.OnlyEnnemies, nullKingdomParty));
            Assert.False(ScopeExtension.IsInScope(Scope.OnlyEnnemies, sameKingdomParty));
            Assert.True(ScopeExtension.IsInScope(Scope.OnlyEnnemies, differentKingdomParty));
            Assert.False(ScopeExtension.IsInScope(Scope.OnlyEnnemies, (MobileParty)null!));
        }

        [Fact]
        public void IsInScope_DispatchesEveryScope_AndRejectsUnknownScope()
        {
            var playerKingdom = new Kingdom { Name = "Vlandia" };
            var playerClan = new Clan { Kingdom = playerKingdom };
            Hero.MainHero = new Hero { Clan = playerClan };
            var party = new PartyBase
            {
                Owner = new Hero { Clan = new Clan { Kingdom = new Kingdom { Name = "Vlandia" } } }
            };

            Assert.True(ScopeExtension.IsInScope(Scope.Everyone, party));
            Assert.False(ScopeExtension.IsInScope(Scope.OnlyPlayer, party));
            Assert.False(ScopeExtension.IsInScope(Scope.OnlyPlayerClan, party));
            Assert.True(ScopeExtension.IsInScope(Scope.OnlyPlayerKingdom, party));
            Assert.False(ScopeExtension.IsInScope(Scope.OnlyEnnemies, party));
            Assert.False(ScopeExtension.IsInScope((Scope)int.MaxValue, party));
        }

        [Fact]
        public void IsInScope_RejectsUnknownScope_ForIncludedPlayer()
        {
            var previousOptions = SubModule.PartySizeReunitedOptions;
            SubModule.PartySizeReunitedOptions = new McMMenu.Options.PartySizeReunitedOptions
            {
                IsPlayerPartyImpacted = true
            };

            try
            {
                var playerParty = new PartyBase
                {
                    Owner = new Hero { IsHumanPlayerCharacter = true }
                };

                Assert.False(ScopeExtension.IsInScope((Scope)int.MaxValue, playerParty));
            }
            finally
            {
                SubModule.PartySizeReunitedOptions = previousOptions;
            }
        }

        [Fact]
        public void EntitiesWithoutKingdom_PreserveTheirExistingEnemyBehavior()
        {
            Hero.MainHero = new Hero { Clan = new Clan { Kingdom = new Kingdom { Name = "Vlandia" } } };
            var party = new PartyBase { Owner = new Hero { Clan = new Clan { Kingdom = null } } };
            var mobileParty = new MobileParty { ActualClan = new Clan { Kingdom = null } };
            var settlement = new Settlement { Owner = new Hero { Clan = new Clan { Kingdom = null } } };

            Assert.True(ScopeExtension.IsInScope(Scope.OnlyEnnemies, party));
            Assert.True(ScopeExtension.IsInScope(Scope.OnlyEnnemies, mobileParty));
            Assert.True(ScopeExtension.IsInScope(Scope.OnlyEnnemies, settlement));
        }

        [Fact]
        public void PlayerWithoutKingdom_UsesSameEnemyRuleForEveryEntityType()
        {
            var playerClan = new Clan { Kingdom = null };
            var otherClan = new Clan { Kingdom = new Kingdom { Name = "Vlandia" } };
            var otherIndependentClan = new Clan { Kingdom = null };
            Hero.MainHero = new Hero { Clan = playerClan };

            Assert.True(ScopeExtension.IsInScope(
                Scope.OnlyEnnemies,
                new PartyBase { Owner = new Hero { Clan = otherClan } }));
            Assert.True(ScopeExtension.IsInScope(
                Scope.OnlyEnnemies,
                new MobileParty { ActualClan = otherClan }));
            Assert.True(ScopeExtension.IsInScope(
                Scope.OnlyEnnemies,
                new Settlement { Owner = new Hero { Clan = otherClan } }));

            Assert.True(ScopeExtension.IsInScope(
                Scope.OnlyEnnemies,
                new PartyBase { Owner = new Hero { Clan = otherIndependentClan } }));
            Assert.True(ScopeExtension.IsInScope(
                Scope.OnlyEnnemies,
                new MobileParty { ActualClan = otherIndependentClan }));
            Assert.True(ScopeExtension.IsInScope(
                Scope.OnlyEnnemies,
                new Settlement { Owner = new Hero { Clan = otherIndependentClan } }));

            Assert.False(ScopeExtension.IsInScope(
                Scope.OnlyEnnemies,
                new PartyBase { Owner = new Hero { Clan = playerClan } }));
            Assert.False(ScopeExtension.IsInScope(
                Scope.OnlyEnnemies,
                new MobileParty { ActualClan = playerClan }));
            Assert.False(ScopeExtension.IsInScope(
                Scope.OnlyEnnemies,
                new Settlement { Owner = new Hero { Clan = playerClan } }));
        }

        [Fact]
        public void Settlement_KingdomScopes_CompareKingdomsByName()
        {
            Hero.MainHero = new Hero { Clan = new Clan { Kingdom = new Kingdom { Name = "Vlandia" } } };
            var settlement = new Settlement
            {
                Owner = new Hero { Clan = new Clan { Kingdom = new Kingdom { Name = "Vlandia" } } }
            };

            Assert.True(ScopeExtension.IsInScope(Scope.OnlyPlayerKingdom, settlement));
            Assert.False(ScopeExtension.IsInScope(Scope.OnlyEnnemies, settlement));
        }
    }
}
