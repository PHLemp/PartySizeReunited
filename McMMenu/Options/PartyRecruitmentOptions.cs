using MCM.Common;

namespace PartySizeReunited.McMMenu.Options
{
    public class PartyRecruitmentOptions
    {
        public bool IsActivate { get; set; }
        public Dropdown<OptionType> Type { get; set; } = new Dropdown<OptionType>(new OptionType[]
        {
            new (OptionTypeEnum.Static),
            new (OptionTypeEnum.Progressive)
        }, 0);
        public int Amount { get; set; }
    }
}
