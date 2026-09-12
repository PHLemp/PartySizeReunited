using PartySizeReunited.HarmonyPatches;
using PartySizeReunited.McMMenu.Options;
using PartySizeReunited.Tests.TestDoubles;
using Xunit;

namespace PartySizeReunited.Tests.Tests
{
    public class NavalShipDeploymentPatchTests
    {
        [Fact]
        public void Postfix_DoesNotThrow_WhenPartyIsNull()
        {
            SubModule.WarSailsOptions = new WarSailsOptions { IsActivate = true, BonusBoats = 3, OnlyApplyToPlayer = false };
            var result = 10;

            var ex = Record.Exception(() => Patch_NavalDLC_ShipDeploymentModel.Postfix(null!, ref result));

            Assert.Null(ex);
            Assert.Equal(10, result);
        }

        [Fact]
        public void Postfix_DoesNotAddBonus_ForNonPlayer_WhenOnlyApplyToPlayer()
        {
            SubModule.WarSailsOptions = new WarSailsOptions { IsActivate = true, BonusBoats = 4, OnlyApplyToPlayer = true };
            var result = 10;
            var party = new FakeParty { LeaderHero = new FakeHero { IsHumanPlayerCharacter = false } };

            Patch_NavalDLC_ShipDeploymentModel.Postfix(party, ref result);

            Assert.Equal(10, result);
        }

        [Fact]
        public void Postfix_AddsBonus_ForNonPlayer_WhenNotRestrictedToPlayer()
        {
            SubModule.WarSailsOptions = new WarSailsOptions { IsActivate = true, BonusBoats = 4, OnlyApplyToPlayer = false };
            var result = 10;
            var party = new FakeParty { LeaderHero = new FakeHero { IsHumanPlayerCharacter = false } };

            Patch_NavalDLC_ShipDeploymentModel.Postfix(party, ref result);

            Assert.Equal(14, result);
        }

        [Fact]
        public void Postfix_AddsBonus_ForPlayer_WhenRestrictedToPlayer()
        {
            SubModule.WarSailsOptions = new WarSailsOptions { IsActivate = true, BonusBoats = 2, OnlyApplyToPlayer = true };
            var result = 10;
            var party = new FakeParty { LeaderHero = new FakeHero { IsHumanPlayerCharacter = true } };

            Patch_NavalDLC_ShipDeploymentModel.Postfix(party, ref result);

            Assert.Equal(12, result);
        }

        [Theory]
        [InlineData(false, false, 14)]
        [InlineData(false, true, 14)]
        [InlineData(true, false, 10)]
        [InlineData(true, true, 14)]
        public void Postfix_AppliesBonusAccordingToOnlyApplyToPlayer(bool onlyApplyToPlayer, bool isPlayer, int expectedResult)
        {
            SubModule.WarSailsOptions = new WarSailsOptions
            {
                IsActivate = true,
                BonusBoats = 4,
                OnlyApplyToPlayer = onlyApplyToPlayer
            };
            var result = 10;
            var party = new FakeParty { LeaderHero = new FakeHero { IsHumanPlayerCharacter = isPlayer } };

            Patch_NavalDLC_ShipDeploymentModel.Postfix(party, ref result);

            Assert.Equal(expectedResult, result);
        }
    }
}
