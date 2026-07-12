using MCM.Abstractions.FluentBuilder;
using MCM.Common;
using PartySizeReunited.McMMenu.Options;

namespace PartySizeReunited.McMMenu
{
    internal class McMPartyRecruitmentSettings
    {
        private static readonly string partyTypeHint = "Progressive = Clan Tier + X + Perks, Static = X; Where X = Your Setting";
        private static readonly string partyLimitHint = "Set the limit of how many parties you can create in your clan.\nWARNING! More you increase your limit, more kingdoms will want to declare wars against you!";
        private static readonly string isActivateHint = "Should party limit option be activated?";

        public static ISettingsBuilder AddPartyRecruitmentSettings(ISettingsBuilder builder, PartyRecruitmentOptions opt)
        {
            return builder
                .CreateGroup("Party Limit", Build);

            void Build(ISettingsPropertyGroupBuilder groupBuilder)
                => groupBuilder
                .AddBool("psr_activate_party_recruitment", "Activate?",
                             new ProxyRef<bool>(() => opt.IsActivate, value => opt.IsActivate = value),
                             propBuilder => propBuilder
                             .SetRequireRestart(false)
                             .SetHintText(isActivateHint)
                             .SetOrder(0)

                )
                .AddDropdown("psr_party_recruitment_type", "Type", 0,
                             new ProxyRef<Dropdown<OptionType>>(() => opt.Type, value => opt.Type = value),
                             propBuilder => propBuilder
                             .SetRequireRestart(false)
                             .SetHintText(partyTypeHint)
                             .SetOrder(1)

                )
                .AddInteger("psr_party_recruitment_amount", "Amount", 0, 500,
                             new ProxyRef<int>(() => opt.Amount, value => opt.Amount = value),
                             propBuilder => propBuilder
                             .SetRequireRestart(false)
                             .SetHintText(partyLimitHint)
                             .SetOrder(2)
                )
                .SetGroupOrder(6);
        }
    }
}
