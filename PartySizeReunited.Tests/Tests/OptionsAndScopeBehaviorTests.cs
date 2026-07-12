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

            Assert.False(options.IsActivate);
            Assert.False(options.IsPlayerPartyImpacted);
            Assert.False(options.NoMoreSupplyIssues);
            Assert.Equal(1f, options.PartyBonusAmnt);
            Assert.Equal(1f, options.PartyInfluenceCost);
            Assert.Equal(0, options.FixedBonusAmnt);
            Assert.Equal(0, options.BonusScope.SelectedIndex);
            Assert.Equal(IScope.Everyone, options.BonusScope.SelectedValue.Scope);
        }

        [Fact]
        public void OptionType_ToString_UsesDescriptionAttribute()
        {
            Assert.Equal("Static", new OptionType(OptionTypeEnum.Static).ToString());
            Assert.Equal("Progressive", new OptionType(OptionTypeEnum.Progressive).ToString());
        }

        [Fact]
        public void ScopeExtension_ToString_UsesDescriptionAttribute()
        {
            Assert.Equal("Everyone (Except player)", new ScopeExtension(IScope.Everyone).ToString());
            Assert.Equal("Only enemies", new ScopeExtension(IScope.Only_ennemies).ToString());
        }

        [Fact]
        public void ScopeExtension_PlayerAndEnemyLogic_WorksForPartyBase()
        {
            var playerKingdom = new Kingdom { Name = "Vlandia" };
            var playerClan = new Clan { Kingdom = playerKingdom };
            Hero.MainHero = new Hero { Clan = playerClan };

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

            Assert.True(ScopeExtension.IsOnlyPlayer(playerParty));
            Assert.False(ScopeExtension.IsOnlyEnnemies(playerParty));
            Assert.True(ScopeExtension.IsOnlyEnnemies(enemyParty));
            Assert.True(ScopeExtension.IsEveryoneExceptPlayer(enemyParty));
        }

        [Fact]
        public void ScopeExtension_PlayerAndEnemyLogic_WorksForMobileParty()
        {
            var playerKingdom = new Kingdom { Name = "Vlandia" };
            var playerClan = new Clan { Kingdom = playerKingdom };
            Hero.MainHero = new Hero { Clan = playerClan };

            var playerMobile = new MobileParty { IsMainParty = true, ActualClan = playerClan };
            var enemyMobile = new MobileParty
            {
                IsMainParty = false,
                ActualClan = new Clan { Kingdom = new Kingdom { Name = "Khuzait" } }
            };

            Assert.True(ScopeExtension.IsOnlyPlayer(playerMobile));
            Assert.False(ScopeExtension.IsOnlyEnnemies(playerMobile));
            Assert.True(ScopeExtension.IsOnlyEnnemies(enemyMobile));
            Assert.True(ScopeExtension.IsEveryoneExceptPlayer(enemyMobile));
        }
    }
}
