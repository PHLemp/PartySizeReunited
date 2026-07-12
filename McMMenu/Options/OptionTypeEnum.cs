using System.ComponentModel;
using System.Reflection;

namespace PartySizeReunited.McMMenu.Options
{
    public class OptionType
    {
        public OptionTypeEnum SelectedValue { get; }

        public OptionType(OptionTypeEnum scope)
        {
            SelectedValue = scope;
        }

        public override string ToString()
        {
            var field = SelectedValue.GetType().GetField(SelectedValue.ToString());
            var attribute = field.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? SelectedValue.ToString();
        }
    }

    public enum OptionTypeEnum
    {
        [Description("Static")]
        Static,
        [Description("Progressive")]
        Progressive
    }
}