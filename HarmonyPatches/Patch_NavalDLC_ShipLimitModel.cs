using HarmonyLib;
using PartySizeReunited.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PartySizeReunited.McMMenu.Options;

namespace PartySizeReunited.HarmonyPatches
{
    internal static class Patch_NavalDLC_ShipLimitModel
    {
        private const string TargetTypeName = "NavalDLC.GameComponents.NavalDLCShipLimitModel";
        private const string MobilePartyTypeName = "TaleWorlds.CampaignSystem.Party.MobileParty";
        private const string ClanTypeName = "TaleWorlds.CampaignSystem.Clan";

        // NavalDLCShipDistributionModel pénalise une flotte dont la capacité
        // totale descend sous 85 % de la taille du parti et tolère jusqu'à 130 %.
        // Viser 110 % laisse la marge nécessaire aux blessés, compagnons et
        // prisonniers sans sortir de la fourchette voulue par le jeu.
        private const double TroopCapacityMargin = 1.10d;

        // Repli utilisé seulement si le parti n'a ni navire ni coque culturelle.
        // Les cultures déclarent des médianes réelles de 34 à 60 (XSLT
        // NavalDLC_SandBoxCore_SPCultures) : 40 reste dans le bas de cette plage.
        private const double FallbackCrewCapacityPerShip = 40d;

        public static void TryApplyPatch(Harmony? harmony)
        {
            if (harmony == null)
            {
                Utils.PrintError("Harmony non initialisé, patch NavalDLCShipLimitModel ignoré");
                return;
            }

            try
            {
                Type? targetType = AppDomain.CurrentDomain.GetAssemblies()
                    .Select(assembly => assembly.GetType(TargetTypeName, false))
                    .FirstOrDefault(type => type != null);

                if (targetType == null)
                {
                    Utils.PrintError("NavalDLCShipLimitModel non trouvé, patch ignoré");
                    return;
                }

                PatchOverload(harmony, targetType, MobilePartyTypeName, nameof(Postfix));
                PatchOverload(harmony, targetType, ClanTypeName, nameof(ClanPostfix));
            }
            catch (Exception ex)
            {
                Utils.PrintError($"Erreur lors du patch NavalDLCShipLimitModel: {ex.Message}");
            }
        }

        private static void PatchOverload(Harmony harmony, Type targetType, string parameterTypeName,
            string postfixName)
        {
            MethodInfo? originalMethod = targetType
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(method =>
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    return method.Name == "GetIdealShipNumber" &&
                           parameters.Length == 1 &&
                           parameters[0].ParameterType.FullName == parameterTypeName;
                });

            if (originalMethod == null)
            {
                Utils.PrintError($"Méthode GetIdealShipNumber({parameterTypeName}) non trouvée");
                return;
            }

            MethodInfo? patchMethod = typeof(Patch_NavalDLC_ShipLimitModel).GetMethod(
                postfixName,
                BindingFlags.Public | BindingFlags.Static);

            if (patchMethod == null)
            {
                Utils.PrintError($"Postfix {postfixName} de NavalDLCShipLimitModel non trouvé");
                return;
            }

            harmony.Patch(originalMethod, postfix: new HarmonyMethod(patchMethod));
        }

        public static void Postfix(object? mobileParty, ref int __result)
        {
            try
            {
                WarSailsOptions options = SubModule.WarSailsOptions;
                if (!options.IsActivate || mobileParty == null)
                {
                    return;
                }

                // Le joueur gère directement sa flotte : cette cible de distribution
                // est destinée uniquement aux parties contrôlées par l'IA.
                if (IsPlayerParty(mobileParty))
                {
                    return;
                }

                __result = GetDynamicTarget(mobileParty, __result, options);
            }
            catch (Exception ex)
            {
                Utils.PrintError($"Erreur dans le patch NavalDLCShipLimitModel: {ex.Message}");
            }
        }

        public static void ClanPostfix(object? clan, ref int __result)
        {
            try
            {
                WarSailsOptions options = SubModule.WarSailsOptions;
                if (!options.IsActivate || clan == null || IsPlayerClan(clan))
                {
                    return;
                }

                object? warPartyComponents = clan.GetType().GetProperty("WarPartyComponents")?.GetValue(clan);
                if (!(warPartyComponents is IEnumerable components))
                {
                    return;
                }

                long target = 0;
                int partyCount = 0;
                foreach (object component in components)
                {
                    object? mobileParty = component?.GetType().GetProperty("MobileParty")?.GetValue(component);
                    if (mobileParty == null)
                    {
                        continue;
                    }

                    target += GetDynamicTarget(mobileParty, 3, options);
                    partyCount++;
                }

                if (partyCount > 0)
                {
                    __result = (int)Math.Min(target, int.MaxValue);
                }
            }
            catch (Exception ex)
            {
                Utils.PrintError($"Erreur dans le patch NavalDLCShipLimitModel pour Clan: {ex.Message}");
            }
        }

        private static int GetDynamicTarget(object mobileParty, int originalTarget, WarSailsOptions options)
        {
            bool isCaravan = GetBoolean(mobileParty, "IsCaravan");
            bool isLordParty = GetBoolean(mobileParty, "IsLordParty");
            bool isBandit = GetBoolean(mobileParty, "IsBandit");
            if (!isCaravan && !isLordParty && !isBandit)
            {
                return originalTarget;
            }

            int troopCount = GetInt32(mobileParty, "Party", "MemberRoster", "TotalManCount");
            if (troopCount <= 0)
            {
                troopCount = GetInt32(mobileParty, "MemberRoster", "TotalManCount");
            }

            object? availableShipHulls = GetValue(mobileParty, "MapFaction", "Culture", "AvailableShipHulls");
            double referenceCrewCapacity = GetReferenceCapacity(
                mobileParty,
                availableShipHulls,
                "TotalCrewCapacity",
                FallbackCrewCapacityPerShip,
                isCaravan,
                isBandit);
            // La flotte doit pouvoir embarquer tout le groupe : réduire la taille
            // des partis est le bon levier pour obtenir des flottes plus petites.
            int crewTarget = (int)Math.Ceiling(troopCount * TroopCapacityMargin / referenceCrewCapacity);
            int dynamicTarget = Math.Max(originalTarget, crewTarget);

            // Aucune cible basée sur la cargaison : NavalDLCInventoryCapacityModel
            // ajoute la capacité des navires à l'inventaire du parti, et une caravane
            // remplit ensuite cet inventaire jusqu'à 90 %. Dimensionner la flotte sur
            // le poids transporté créerait donc une boucle « acheter un navire ->
            // plus de capacité -> plus de marchandises -> acheter un navire ».

            int maximum = Math.Max(originalTarget, options.MaximumIdealShips);
            return Clamp(dynamicTarget, originalTarget, maximum);
        }

        private static double GetReferenceCapacity(object mobileParty, object? availableShipHulls,
            string capacityName, double fallbackCapacity, bool isCaravan, bool isBandit)
        {
            // 1. Les navires réellement possédés sont la seule donnée certaine.
            double actualFleetCapacity = GetAverageShipCapacity(mobileParty, capacityName);
            if (actualFleetCapacity > 0d)
            {
                return actualFleetCapacity;
            }

            // 2. Les coques culturelles, quand la campagne en déclare réellement.
            double medianCapacity = GetMedian(availableShipHulls, capacityName, isCaravan, isBandit);
            if (medianCapacity > 0d)
            {
                return medianCapacity;
            }

            // 3. Estimation prudente pour un parti sans navire ni coque déclarée.
            return fallbackCapacity;
        }

        private static double GetAverageShipCapacity(object mobileParty, string hullPropertyName)
        {
            if (!(GetValue(mobileParty, "Ships") is IEnumerable ships))
            {
                return 0d;
            }

            double totalCapacity = 0d;
            int shipCount = 0;
            foreach (object ship in ships)
            {
                double capacity = GetDouble(ship, "ShipHull", hullPropertyName);
                if (capacity <= 0d)
                {
                    continue;
                }

                totalCapacity += capacity;
                shipCount++;
            }

            return shipCount == 0 ? 0d : totalCapacity / shipCount;
        }

        private static double GetMedian(object? values, string propertyName, bool isCaravan, bool isBandit)
        {
            if (!(values is IEnumerable enumerable))
            {
                return 0d;
            }

            var capacities = new List<double>();
            foreach (object value in enumerable)
            {
                if (!IsEligibleHull(value, isCaravan, isBandit))
                {
                    continue;
                }

                double capacity = GetDouble(value, propertyName);
                if (capacity > 0d)
                {
                    capacities.Add(capacity);
                }
            }

            if (capacities.Count == 0)
            {
                return 0d;
            }

            capacities.Sort();
            int middle = capacities.Count / 2;
            return capacities.Count % 2 == 0
                ? (capacities[middle - 1] + capacities[middle]) / 2d
                : capacities[middle];
        }

        private static bool IsEligibleHull(object hull, bool isCaravan, bool isBandit)
        {
            object? type = GetValue(hull, "Type");
            if (type == null)
            {
                return true;
            }

            int shipType = Convert.ToInt32(type);
            // CaravansCampaignBehavior.BuyShips achète des coques légères et, pour
            // les convois d'élite, des coques moyennes ; GetShipPriority écarte les
            // coques lourdes. CanPartyTakeShip interdit également les coques
            // lourdes (2) aux bandits.
            if (isCaravan || isBandit)
            {
                return shipType != 2;
            }

            return true;
        }

        private static object? GetValue(object? source, params string[] propertyPath)
        {
            object? current = source;
            foreach (string propertyName in propertyPath)
            {
                if (current == null)
                {
                    return null;
                }

                current = current.GetType().GetProperty(propertyName)?.GetValue(current);
            }

            return current;
        }

        private static bool GetBoolean(object source, string propertyName)
            => GetValue(source, propertyName) is bool value && value;

        private static int GetInt32(object source, params string[] propertyPath)
            => GetValue(source, propertyPath) is int value ? value : 0;

        private static double GetDouble(object? source, params string[] propertyPath)
        {
            object? value = GetValue(source, propertyPath);
            return value == null ? 0d : Convert.ToDouble(value);
        }

        private static int Clamp(int value, int minimum, int maximum)
            => Math.Max(minimum, Math.Min(value, maximum));

        private static bool IsPlayerParty(object mobileParty)
        {
            Type partyType = mobileParty.GetType();
            PropertyInfo? isMainPartyProperty = partyType.GetProperty("IsMainParty");
            if (isMainPartyProperty?.GetValue(mobileParty) is bool isMainParty && isMainParty)
            {
                return true;
            }

            object? leaderHero = partyType.GetProperty("LeaderHero")?.GetValue(mobileParty);
            return leaderHero?.GetType().GetProperty("IsHumanPlayerCharacter")?.GetValue(leaderHero) is bool isPlayer &&
                   isPlayer;
        }

        private static bool IsPlayerClan(object clan)
        {
            Type clanType = clan.GetType();
            object? playerClan = clanType
                .GetProperty("PlayerClan", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                ?.GetValue(null);

            if (ReferenceEquals(playerClan, clan))
            {
                return true;
            }

            object? leader = clanType.GetProperty("Leader")?.GetValue(clan);
            return leader?.GetType().GetProperty("IsHumanPlayerCharacter")?.GetValue(leader) is bool isPlayer &&
                   isPlayer;
        }
    }
}

