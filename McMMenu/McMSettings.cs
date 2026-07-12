using MCM.Abstractions.FluentBuilder;

namespace PartySizeReunited.McMMenu
{
    internal static class McMSettings
    {
        private const string SettingsId = "PartySizeReunited";
        private const string DisplayedName = "Party Size Reunited";

        public static ISettingsBuilder InitMcMSettings()
        {
            return BaseSettingsBuilder.Create(SettingsId, DisplayedName)!
                .SetFormat("json")
                .SetFolderName("PartySizeReunited");
        }
    }
}
