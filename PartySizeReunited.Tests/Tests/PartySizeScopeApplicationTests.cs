using System.Collections.Generic;
using MCM.Common;
using PartySizeReunited.McMMenu.Options;
using PartySizeReunited.Models;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using Xunit;

namespace PartySizeReunited.Tests.Tests
{
    public class PartySizeScopeApplicationTests : IDisposable
    {
        private const float BaseLimit = 10;
        private const float DoubledLimit = 20;

        public PartySizeScopeApplicationTests()
        {
            SubModule.PartySizeReunitedOptions = new PartySizeReunitedOptions();
            Hero.MainHero = null;
        }

        public static IEnumerable<object[]> ScopeApplicationCases()
        {
            foreach (var isPlayerPartyImpacted in new[] { false, true })
            {
                yield return new object[] { Scope.Everyone, isPlayerPartyImpacted, "player", isPlayerPartyImpacted ? DoubledLimit : BaseLimit };
                yield return new object[] { Scope.Everyone, isPlayerPartyImpacted, "clan", DoubledLimit };
                yield return new object[] { Scope.Everyone, isPlayerPartyImpacted, "kingdom", DoubledLimit };
                yield return new object[] { Scope.Everyone, isPlayerPartyImpacted, "enemy", DoubledLimit };

                yield return new object[] { Scope.OnlyPlayer, isPlayerPartyImpacted, "player", isPlayerPartyImpacted ? DoubledLimit : BaseLimit };
                yield return new object[] { Scope.OnlyPlayer, isPlayerPartyImpacted, "clan", BaseLimit };
                yield return new object[] { Scope.OnlyPlayer, isPlayerPartyImpacted, "kingdom", BaseLimit };
                yield return new object[] { Scope.OnlyPlayer, isPlayerPartyImpacted, "enemy", BaseLimit };

                yield return new object[] { Scope.OnlyPlayerClan, isPlayerPartyImpacted, "player", isPlayerPartyImpacted ? DoubledLimit : BaseLimit };
                yield return new object[] { Scope.OnlyPlayerClan, isPlayerPartyImpacted, "clan", DoubledLimit };
                yield return new object[] { Scope.OnlyPlayerClan, isPlayerPartyImpacted, "kingdom", BaseLimit };
                yield return new object[] { Scope.OnlyPlayerClan, isPlayerPartyImpacted, "enemy", BaseLimit };

                yield return new object[] { Scope.OnlyPlayerKingdom, isPlayerPartyImpacted, "player", isPlayerPartyImpacted ? DoubledLimit : BaseLimit };
                yield return new object[] { Scope.OnlyPlayerKingdom, isPlayerPartyImpacted, "clan", DoubledLimit };
                yield return new object[] { Scope.OnlyPlayerKingdom, isPlayerPartyImpacted, "kingdom", DoubledLimit };
                yield return new object[] { Scope.OnlyPlayerKingdom, isPlayerPartyImpacted, "enemy", BaseLimit };

                yield return new object[] { Scope.OnlyEnnemies, isPlayerPartyImpacted, "player", isPlayerPartyImpacted ? DoubledLimit : BaseLimit };
                yield return new object[] { Scope.OnlyEnnemies, isPlayerPartyImpacted, "clan", BaseLimit };
                yield return new object[] { Scope.OnlyEnnemies, isPlayerPartyImpacted, "kingdom", BaseLimit };
                yield return new object[] { Scope.OnlyEnnemies, isPlayerPartyImpacted, "enemy", DoubledLimit };
            }
        }

        [Theory]
        [MemberData(nameof(ScopeApplicationCases))]
        public void GetPartyMemberSizeLimit_AppliesBonusForExpectedScopeAndPlayerSetting(
            Scope scope,
            bool isPlayerPartyImpacted,
            string partyKind,
            float expectedLimit)
        {
            var playerKingdom = new Kingdom { Name = "Vlandia" };
            var playerClan = new Clan { Kingdom = playerKingdom };
            Hero.MainHero = new Hero { IsHumanPlayerCharacter = true, Clan = playerClan };
            SubModule.PartySizeReunitedOptions = new PartySizeReunitedOptions
            {
                IsActivate = true,
                IsPlayerPartyImpacted = isPlayerPartyImpacted,
                PartyBonusAmnt = 2f,
                BonusScope = new Dropdown<ScopeExtension>(new[] { new ScopeExtension(scope) }, 0)
            };

            var partySize = new PartySize(new List<DefaultPartySizeLimitModel> { new FixedPartySizeLimitModel(BaseLimit) });

            var result = partySize.GetPartyMemberSizeLimit(CreateParty(partyKind, playerClan, playerKingdom));

            Assert.Equal(expectedLimit, result.ResultNumber);
        }

        [Fact]
        public void GetPartyMemberSizeLimit_UsesGarrisonOptionsForSettlements()
        {
            SubModule.PartySizeReunitedOptions = new PartySizeReunitedOptions
            {
                IsActivate = true,
                IsGarrisonActivate = true,
                BonusScope = new Dropdown<ScopeExtension>(new[] { new ScopeExtension(Scope.OnlyPlayer) }, 0),
                CaravanFixedBonus = 99,
                PartyGarrisonFixedBonus = 5
            };
            var party = new PartyBase
            {
                IsSettlement = true,
                Settlement = new TaleWorlds.CampaignSystem.Settlements.Settlement()
            };
            var partySize = new PartySize(new List<DefaultPartySizeLimitModel> { new FixedPartySizeLimitModel(BaseLimit) });

            var result = partySize.GetPartyMemberSizeLimit(party);

            Assert.Equal(15, result.ResultNumber);
        }

        public void Dispose()
        {
            SubModule.PartySizeReunitedOptions = new PartySizeReunitedOptions();
            Hero.MainHero = null;
        }

        private static PartyBase CreateParty(string partyKind, Clan playerClan, Kingdom playerKingdom)
        {
            return partyKind switch
            {
                "player" => new PartyBase { Owner = new Hero { IsHumanPlayerCharacter = true, Clan = playerClan } },
                "clan" => new PartyBase { Owner = new Hero { Clan = playerClan } },
                "kingdom" => new PartyBase { Owner = new Hero { Clan = new Clan { Kingdom = playerKingdom } } },
                "enemy" => new PartyBase { Owner = new Hero { Clan = new Clan { Kingdom = new Kingdom { Name = "Aserai" } } } },
                _ => throw new ArgumentOutOfRangeException(nameof(partyKind), partyKind, null)
            };
        }

        private sealed class FixedPartySizeLimitModel : DefaultPartySizeLimitModel
        {
            private readonly float _limit;

            public FixedPartySizeLimitModel(float limit)
            {
                _limit = limit;
            }

            public override ExplainedNumber GetPartyMemberSizeLimit(PartyBase party, bool includeDescriptions = false)
            {
                return new ExplainedNumber(_limit);
            }
        }
    }
}

