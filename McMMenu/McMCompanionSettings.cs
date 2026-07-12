using MCM.Abstractions.FluentBuilder;
using MCM.Common;
using PartySizeReunited.McMMenu.Options;

namespace PartySizeReunited.McMMenu
{
    internal class McMCompanionSettings
    {
        private const string companionTypeHint = "Progressive = Clan Tier + X + Perks, Static = X; Where X = Your Setting";
        private const string companionLimitHint = "Set the limit of how many companions you can recruit in your party.";
        private const string isActivateHint = "Companion limit settings.";

        public static ISettingsBuilder AddCompanionsSettings(ISettingsBuilder builder, CompanionsOptions opt)
        {
            return builder
                .CreateGroup("Companions Limit", BuildCompanions);

            void BuildCompanions(ISettingsPropertyGroupBuilder builder)
                => builder
                .AddToggle("psr_activate_companions", "Enable companion limits",
                             new ProxyRef<bool>(() => opt.IsActivate, value => opt.IsActivate = value),
                             propBuilder => propBuilder
                             .SetRequireRestart(false)
                             .SetHintText(isActivateHint)
                             .SetOrder(0)

                )
                .AddDropdown("psr_companions_type", "Type", 0,
                             new ProxyRef<Dropdown<OptionType>>(() => opt.Type, value => opt.Type = value),
                             propBuilder => propBuilder
                             .SetRequireRestart(false)
                             .SetHintText(companionTypeHint)
                             .SetOrder(1)

                )
                .AddInteger("psr_companions_amount", "Amount", 0, 500,
                             new ProxyRef<int>(() => opt.Amount, value => opt.Amount = value),
                             propBuilder => propBuilder
                             .SetRequireRestart(false)
                             .SetHintText(companionLimitHint)
                             .SetOrder(2)
                )
                .SetGroupOrder(5);
        }
    }
}
