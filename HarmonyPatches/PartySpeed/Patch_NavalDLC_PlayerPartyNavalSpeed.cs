using HarmonyLib;
using PartySizeReunited.Services;
using System;
using System.Linq;
using System.Reflection;
using PartySizeReunited.McMMenu.Options;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace PartySizeReunited.HarmonyPatches.PartySpeed
{
    class Patch_NavalDLC_PlayerPartyNavalSpeed
    {
        public static void TryApplyPatch(Harmony harmony)
        {
            try
            {
                // Chercher le type dans les assemblies chargés
                Type targetType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.Name == "NavalDLCPartySpeedCalculationModel");

                if (targetType == null)
                {
                    Utils.PrintError("NavalDLCPartySpeedCalculationModel non trouvé, patch ignoré");
                    return;
                }

                // Obtenir la méthode à patcher
                MethodInfo calculateNavalBaseSpeedMethod = targetType.GetMethod("CalculateNavalBaseSpeed",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                MethodInfo CalculateFinalSpeedMethod = targetType.GetMethod("CalculateFinalSpeed",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

                if (calculateNavalBaseSpeedMethod == null || CalculateFinalSpeedMethod == null)
                {
                    Utils.PrintError("Méthode CalculateNavalBaseSpeed | CalculateFinalSpeedMethod non trouvée");
                    return;
                }

                // Obtenir la méthode de patch
                MethodInfo patchMethod = typeof(Patch_NavalDLC_PlayerPartyNavalSpeed).GetMethod(nameof(Postfix),
                    BindingFlags.Public | BindingFlags.Static);

                // Appliquer le patch en Postfix
                harmony.Patch(calculateNavalBaseSpeedMethod, postfix: new HarmonyMethod(patchMethod));
                harmony.Patch(CalculateFinalSpeedMethod, postfix: new HarmonyMethod(patchMethod));
            }
            catch (Exception ex)
            {
                Utils.PrintError($"Erreur lors du patch NavalDLCPartySpeedCalculationModel: {ex.Message}");
            }
        }

        public static void Postfix(MobileParty mobileParty, ref ExplainedNumber __result)
        {
            if (!SubModule.PartySizeReunitedOptions.IsActivate)
            {
                return;
            }

            if (mobileParty is { IsCurrentlyAtSea: true, IsCaravan: true })
            {
                var currentShipCapacity = mobileParty.Ships.Sum(x => x.ShipHull.TotalCrewCapacity);
                var totalNbTroops = mobileParty.Party.MemberRoster.TotalManCount;
                var nbNeededCapacity = totalNbTroops - currentShipCapacity;
                // Value between 0 and 100. More nbNeededCapacity is high, more ratio is near 100
                var ratio = Math.Ceiling((nbNeededCapacity > 0 ? (float) nbNeededCapacity / totalNbTroops : 0) * 100);
                nbNeededCapacity = nbNeededCapacity < 0 ? 0 : nbNeededCapacity;
                var anyShipHull = mobileParty.MapFaction.Culture.AvailableShipHulls
                    .GetRandomElementWithPredicate(e => e.TotalCrewCapacity >= ratio) ?? mobileParty.MapFaction.Culture.AvailableShipHulls.GetRandomElement();

                var neededShips = (int) Math.Ceiling((float) nbNeededCapacity / anyShipHull.TotalCrewCapacity);
                for (var i = 0; i < neededShips; i++)
                {
                    _ = new Ship(anyShipHull)
                    {
                        Owner = mobileParty.Party
                    };
                }
            }
            
            if (mobileParty.Party.LeaderHero == null || !mobileParty.Party.LeaderHero.IsHumanPlayerCharacter)
            {
                return;
            }

            PartySizeReunitedOptions options = SubModule.PartySizeReunitedOptions;
            TextObject description = new("PartySizeReunited modifier");
            if (options.PlayerPartySpeedFixedBonus != 0)
            {
                __result.Add(
                    options.PlayerPartySpeedFixedBonus,
                    description
                );
            }
            else
            {
                __result.AddFactor(
                    options.PlayerPartySpeedMultiBonus,
                    description
                    );
            }
        }
    }
}
