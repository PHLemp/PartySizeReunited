using PartySizeReunited.McMMenu.Options;
namespace PartySizeReunited
{

    public static class SubModule
    {
        public static PartySizeReunitedOptions PartySizeReunitedOptions { get; set; } = new PartySizeReunitedOptions();
        public static WarSailsOptions WarSailsOptions { get; set; } = new WarSailsOptions();
    }
}

namespace PartySizeReunited.Services
{
    internal static class Utils
    {
        public static void Print(string message, float red = 1, float green = 1, float blue = 1, float alpha = 1)
        {
        }

        public static void PrintError(string message, float red = 1, float green = 0, float blue = 0, float alpha = 0)
        {
        }
    }
}
