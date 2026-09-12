using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;

namespace PartySizeReunited.Services
{
    public static class ConsoleCommands
    {
        [CommandLineFunctionality.CommandLineArgumentFunction("world_war", "psr")]
        public static string WorldWar(List<string> args)
        {
            var kingdoms = new List<Kingdom>(Kingdom.All);
            for (var i = 0; i < kingdoms.Count; i++)
            {
                for (var j = i + 1; j < kingdoms.Count; j++)
                {
                    var kingdomA = kingdoms[i];
                    var kingdomB = kingdoms[j];
                    if (!kingdomA.IsAtWarWith(kingdomB))
                    {
                        FactionManager.DeclareWar(kingdomA, kingdomB);
                    }
                }
            }
            return "All kingdoms are now at war with each other.";
        }
        
        [CommandLineFunctionality.CommandLineArgumentFunction("world_peace", "psr")]
        public static string WorldPeace(List<string> args)
        {
            var kingdoms = new List<Kingdom>(Kingdom.All);
            for (var i = 0; i < kingdoms.Count; i++)
            {
                for (var j = i + 1; j < kingdoms.Count; j++)
                {
                    var kingdomA = kingdoms[i];
                    var kingdomB = kingdoms[j];
                    if (kingdomA.IsAtWarWith(kingdomB))
                    {
                        MakePeaceAction.Apply(kingdomA, kingdomB);
                    }
                }
            }

            return "All kingdoms are now at peace with each other.";
        }
    }
}