using MCM.Abstractions.FluentBuilder;
using MCM.Common;
using PartySizeReunited.McMMenu.Options;

namespace PartySizeReunited.McMMenu
{
    internal class McMWorkshopLimitSettings
    {
        private static readonly string fixedBonusHint = "Adds a fixed bonus to the default workshop limit (default is clan tier + 1).";
        private static readonly string isActivateHint = "Should workshop limit option be activated?";

        public static ISettingsBuilder AddWorkshopLimitSettings(ISettingsBuilder builder, WorkshopLimitOptions opt)
        {
            return builder
                .CreateGroup("Workshop Limit", Build);

            void Build(ISettingsPropertyGroupBuilder groupBuilder)
                => groupBuilder
                .AddBool("psr_activate_workshop_limit", "Activate?",
                             new ProxyRef<bool>(() => opt.IsActivate, value => opt.IsActivate = value),
                             propBuilder => propBuilder
                             .SetRequireRestart(false)
                             .SetHintText(isActivateHint)
                             .SetOrder(0)

                )
                .AddInteger("psr_workshop_limit_amount", "Fixed bonus", 0, 50,
                             new ProxyRef<int>(() => opt.Amount, value => opt.Amount = value),
                             propBuilder => propBuilder
                             .SetRequireRestart(false)
                             .SetHintText(fixedBonusHint)
                             .SetOrder(1)
                )
                .SetGroupOrder(6);
        }
    }
}

