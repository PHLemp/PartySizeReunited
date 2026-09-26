using PartySizeReunited.McMMenu.Options;
using PartySizeReunited.Models;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using Xunit;

namespace PartySizeReunited.Tests.Tests
{
    public class OptionsAndScopeBehaviorTests
    {
        [Fact]
        public void PartySizeReunitedOptions_HaveExpectedDefaults()
        {
            var options = new PartySizeReunitedOptions();

            Assert.True(options.IsActivate);
            Assert.False(options.IsWageActivate);
            Assert.False(options.IsPrisonerActivate);
            Assert.False(options.IsGarrisonActivate);
            Assert.False(options.IsCaravanActivate);
            Assert.False(options.IsPlayerPartyImpacted);
            Assert.False(options.NoMoreSupplyIssues);
            Assert.Equal(1f, options.PartyBonusAmnt);
            Assert.Equal(1f, options.PartyInfluenceCost);
            Assert.Equal(0, options.FixedBonusAmnt);
            Assert.Equal(0, options.BonusScope.SelectedIndex);
            Assert.Equal(Scope.Everyone, options.BonusScope.SelectedValue.Scope);
        }

        [Fact]
        public void OptionType_ToString_UsesDescriptionAttribute()
        {
            Assert.Equal("Static", new OptionType(OptionTypeEnum.Static).ToString());
            Assert.Equal("Progressive", new OptionType(OptionTypeEnum.Progressive).ToString());
        }

        [Fact]
        public void WarSailsOptions_HaveExpectedDefaults()
        {
            var options = new WarSailsOptions();

            Assert.False(options.IsActivate);
            Assert.False(options.OnlyApplyToPlayer);
            Assert.Equal(0, options.BonusBoats);
            Assert.Equal(250, options.MaximumIdealShips);
        }

        [Fact]
        public void ScopeExtension_ToString_UsesDescriptionAttribute()
        {
            Assert.Equal("Everyone (Except player)", new ScopeExtension(Scope.Everyone).ToString());
            Assert.Equal("Only enemies", new ScopeExtension(Scope.OnlyEnnemies).ToString());
        }

        [Fact]
        public void ScopeExtension_PlayerAndEnemyLogic_WorksForPartyBase()
        {
            var playerKingdom = new Kingdom { Name = "Vlandia" };
            var playerClan = new Clan { Kingdom = playerKingdom };
            var previousOptions = SubModule.PartySizeReunitedOptions;
            var previousMainHero = Hero.MainHero;
            Hero.MainHero = new Hero { Clan = playerClan };
            SubModule.PartySizeReunitedOptions = new PartySizeReunitedOptions
            {
                IsPlayerPartyImpacted = true
            };

            var playerParty = new PartyBase
            {
                Owner = new Hero { IsHumanPlayerCharacter = true, Clan = playerClan }
            };

            var enemyParty = new PartyBase
            {
                Owner = new Hero
                {
                    IsHumanPlayerCharacter = false,
                    Clan = new Clan { Kingdom = new Kingdom { Name = "Aserai" } }
                }
            };

            try
            {
                Assert.True(ScopeExtension.IsInScope(Scope.OnlyPlayer, playerParty));
                Assert.True(ScopeExtension.IsInScope(Scope.OnlyEnnemies, playerParty));
                Assert.True(ScopeExtension.IsInScope(Scope.OnlyEnnemies, enemyParty));
                Assert.True(ScopeExtension.IsInScope(Scope.Everyone, enemyParty));
            }
            finally
            {
                SubModule.PartySizeReunitedOptions = previousOptions;
                Hero.MainHero = previousMainHero;
            }
        }

        [Fact]
        public void ScopeExtension_PlayerAndEnemyLogic_WorksForMobileParty()
        {
            var playerKingdom = new Kingdom { Name = "Vlandia" };
            var playerClan = new Clan { Kingdom = playerKingdom };
            var previousOptions = SubModule.PartySizeReunitedOptions;
            var previousMainHero = Hero.MainHero;
            Hero.MainHero = new Hero { Clan = playerClan };
            SubModule.PartySizeReunitedOptions = new PartySizeReunitedOptions
            {
                IsPlayerPartyImpacted = true
            };

            var playerMobile = new MobileParty { IsMainParty = true, ActualClan = playerClan };
            var enemyMobile = new MobileParty
            {
                IsMainParty = false,
                ActualClan = new Clan { Kingdom = new Kingdom { Name = "Khuzait" } }
            };

            try
            {
                Assert.True(ScopeExtension.IsInScope(Scope.OnlyPlayer, playerMobile));
                Assert.True(ScopeExtension.IsInScope(Scope.OnlyEnnemies, playerMobile));
                Assert.True(ScopeExtension.IsInScope(Scope.OnlyEnnemies, enemyMobile));
                Assert.True(ScopeExtension.IsInScope(Scope.Everyone, enemyMobile));
            }
            finally
            {
                SubModule.PartySizeReunitedOptions = previousOptions;
                Hero.MainHero = previousMainHero;
            }
        }
    }
}
