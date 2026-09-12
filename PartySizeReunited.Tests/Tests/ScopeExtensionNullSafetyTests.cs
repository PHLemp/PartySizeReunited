using PartySizeReunited.Models;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using Xunit;

namespace PartySizeReunited.Tests.Tests
{
    public class ScopeExtensionNullSafetyTests
    {
        [Fact]
        public void PartyBase_IsInScope_ReturnsFalse_WhenPartyIsNull()
        {
            foreach (Scope scope in Enum.GetValues<Scope>())
            {
                Assert.False(ScopeExtension.IsInScope(scope, (PartyBase)null!));
            }
        }

        [Fact]
        public void MobileParty_IsInScope_ReturnsFalse_WhenPartyIsNull()
        {
            foreach (Scope scope in Enum.GetValues<Scope>())
            {
                Assert.False(ScopeExtension.IsInScope(scope, (MobileParty)null!));
            }
        }

        [Fact]
        public void Settlement_IsInScope_ReturnsFalse_WhenSettlementIsNull()
        {
            foreach (Scope scope in Enum.GetValues<Scope>())
            {
                Assert.False(ScopeExtension.IsInScope(scope, (Settlement)null!));
            }
        }
    }
}
