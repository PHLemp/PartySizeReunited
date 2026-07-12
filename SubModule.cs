using HarmonyLib;
using MCM.Abstractions.Base.Global;
using MCM.Abstractions.FluentBuilder;
using PartySizeReunited.HarmonyPatches;
using PartySizeReunited.HarmonyPatches.PartySpeed;
using PartySizeReunited.HarmonyPatches.Wage;
using PartySizeReunited.McMMenu;
using PartySizeReunited.McMMenu.Options;
using PartySizeReunited.Services;
using TaleWorlds.Core;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;

namespace PartySizeReunited
{
    public class SubModule : MBSubModuleBase
    {
        private const string ModuleId = "PartySizeReunited";

        public static readonly WarSailsOptions WarSailsOptions = new();
        public static readonly PartySizeReunitedOptions PartySizeReunitedOptions = new();
        public static readonly CompanionsOptions CompanionsOptions = new();
        public static readonly PartyRecruitmentOptions PartyRecruitmentOptions = new();
        public static readonly WorkshopLimitOptions WorkshopLimitOptions = new();

        private static bool _isWarSailsModulePresent;
        private static Harmony? _harmony;
        private static FluentGlobalSettings? _settings;

        private bool _initOnce;

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);

            // Découvrir tous les modèles de taille de party
            var discoveryService = new PartySizeModelDiscoveryService();
            var partySizeModels = discoveryService.DiscoverPartySizeModels();

            // Injecter les modèles dans notre PartySize
            gameStarterObject.AddModel(new PartySize(partySizeModels));
        }

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            if (_harmony != null)
                return;

            _harmony = new Harmony(ModuleId);
            _harmony.PatchAll(typeof(SubModule).Assembly);
        }

        protected override void OnSubModuleUnloaded()
        {
            _harmony?.UnpatchAll(ModuleId);
            _harmony = null;

            base.OnSubModuleUnloaded();
        }

        public override void OnInitialState()
        {
            base.OnInitialState();

            if (_initOnce)
            {
                return;
            }

            _initOnce = true;

            _isWarSailsModulePresent = ModuleHelper.GetActiveModules()
                .Exists(module =>
                    module.IsOfficial &&
                    module.Category == ModuleCategory.Singleplayer &&
                    module.Id == "NavalDLC"
                );

            ISettingsBuilder builder = McMSettings.InitMcMSettings();
            McMPartySizeReunitedSettings.AddPartySizeSettings(builder, PartySizeReunitedOptions);
            AddMoreSettings(builder);
            AddBonusSettings(builder);
            _settings = builder.BuildAsGlobal();
            _settings?.Register();
        }

        private static void AddMoreSettings(ISettingsBuilder builder)
        {
            if (!_isWarSailsModulePresent) return;

            // Add WarSails mod options
            McMWarSailsSettings.AddWarsailsSettings(builder, WarSailsOptions);

            // Patch WarSails deployment method
            Patch_NavalDLC_ShipDeploymentModel.TryApplyPatch(_harmony);
            Patch_NavalDLC_PartyWage.TryApplyPatch(_harmony);
            Patch_NavalDLC_PlayerPartyNavalSpeed.TryApplyPatch(_harmony);

            Utils.Print("PartySizeReunited configured for WarSails DLC");
        }

        private static void AddBonusSettings(ISettingsBuilder builder)
        {
            McMCompanionSettings.AddCompanionsSettings(builder, CompanionsOptions);
            McMPartyRecruitmentSettings.AddPartyRecruitmentSettings(builder, PartyRecruitmentOptions);
            McMWorkshopLimitSettings.AddWorkshopLimitSettings(builder, WorkshopLimitOptions);
        }
    }
}

