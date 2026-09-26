using MCM.Abstractions.FluentBuilder;
using MCM.Common;
using PartySizeReunited.McMMenu.Options;

namespace PartySizeReunited.McMMenu
{
    internal static class McMWarSailsSettings
    {
        private const string MoreBoatHint = "Increase ship deployment limit by the amount selected.\n(You can NOT have more than 8 ships at once in battle)";
        private const string MaximumIdealShipsHint = "Safety limit only. AI fleets always aim to carry every troop of the party, so lower this value only to protect performance. To get smaller fleets, reduce party sizes instead.";
        private const string OnlyApplyToPlayerHint = "Apply the ship deployment bonus only to the player. This does not affect the AI party ship bonus.";
        private const string IsActivateHint = "WarSails ship bonuses.";

        public static ISettingsBuilder AddWarsailsSettings(ISettingsBuilder builder, WarSailsOptions opt)
        {
            return builder
                .CreateGroup("WarSails DLC", BuildWarSails);

            void BuildWarSails(ISettingsPropertyGroupBuilder builder)
                => builder
                .AddToggle("psr_warsails_activate", "Enable ship bonuses",
                             new ProxyRef<bool>(() => opt.IsActivate, value => opt.IsActivate = value),
                             propBuilder => propBuilder
                             .SetRequireRestart(false)
                             .SetHintText(IsActivateHint)
                             .SetOrder(0)
                             )
                .AddBool("psr_only_apply_to_player", "Deployment bonus: player only?",
                             new ProxyRef<bool>(() => opt.OnlyApplyToPlayer, value => opt.OnlyApplyToPlayer = value),
                             propBuilder => propBuilder
                             .SetRequireRestart(false)
                             .SetHintText(OnlyApplyToPlayerHint)
                             .SetOrder(1)
                             )
                .AddInteger("psr_more_boat",
                            "Deployment bonus boat",
                            0,
                            8,
                            new ProxyRef<int>(() => opt.BonusBoats, value => opt.BonusBoats = value),
                            propBuilder => propBuilder
                            .SetRequireRestart(false)
                            .SetHintText(MoreBoatHint)
                            .SetOrder(2)
                            )
                .AddInteger("psr_maximum_ideal_ships",
                            "AI fleet safety limit",
                            3,
                            500,
                            new ProxyRef<int>(() => opt.MaximumIdealShips, value => opt.MaximumIdealShips = value),
                            propBuilder => propBuilder
                            .SetRequireRestart(false)
                            .SetHintText(MaximumIdealShipsHint)
                            .SetOrder(3)
                            )
                    .SetGroupOrder(7);
        }
    }
}
