using HarmonyLib;
using MCM.Common;
using PartySizeReunited.Models;
using PartySizeReunited.Services;
using System;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Localization;

namespace PartySizeReunited.HarmonyPatches.Wage
{
    class Patch_NavalDLC_PartyWage
    {
        private static readonly TextObject partySizeBonusText = new TextObject("Party Size Reunited wage modifier");
        private static readonly string className = "NavalDLCPartyWageModel";
        private static readonly string methodName = "GetTotalWage";

        public static void TryApplyPatch(Harmony harmony)
        {
            try
            {
                // Chercher le type dans les assemblies chargés
                Type targetType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.Name == className);

                if (targetType == null)
                {
                    Utils.PrintError(className + " non trouvé, patch ignoré");
                    return;
                }

                // Obtenir la méthode à patcher
                MethodInfo originalMethod = targetType.GetMethod(methodName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

                if (originalMethod == null)
                {
                    Utils.PrintError("Méthode " + methodName + " non trouvée");
                    return;
                }

                // Obtenir la méthode de patch
                MethodInfo patchMethod = typeof(Patch_NavalDLC_PartyWage).GetMethod(nameof(Postfix),
                    BindingFlags.Public | BindingFlags.Static);

                // Appliquer le patch en Postfix
                harmony.Patch(originalMethod, postfix: new HarmonyMethod(patchMethod));
            }
            catch (Exception ex)
            {
                Utils.PrintError($"Erreur lors du patch " + className + ": {ex.Message}");
            }
        }
        public static void Postfix(MobileParty mobileParty, ref ExplainedNumber __result)
        {
            if (!SubModule.PartySizeReunitedOptions.IsActivate || !SubModule.PartySizeReunitedOptions.IsWageActivate)
                return;

            if (mobileParty.IsGarrison)
                return;

            Dropdown<ScopeExtension> bonusScope = SubModule.PartySizeReunitedOptions.BonusScope;
            IScope selectedScope = bonusScope.SelectedValue.Scope;
            float bonusPercentage = SubModule.PartySizeReunitedOptions.PartyWageMultiBonus;
            int fixedBonus = SubModule.PartySizeReunitedOptions.PartyWageFixedBonus;
            bool isPlayerImpacted = SubModule.PartySizeReunitedOptions.IsPlayerPartyImpacted;

            float newValue = fixedBonus != 0 ?
                __result.ResultNumber + fixedBonus : // If fixedBonus is set
                (float)Math.Round(__result.ResultNumber * bonusPercentage); // If not, we take the multiplicator

            float valueToApply = newValue - __result.ResultNumber;

            switch (selectedScope)
            {
                case IScope.Everyone:
                    if (ScopeExtension.IsEveryoneExceptPlayer(mobileParty))
                    {
                        __result.Add(valueToApply, partySizeBonusText);
                    }
                    break;

                case IScope.Only_player_clan:
                    if (ScopeExtension.IsOnlyPlayerClan(mobileParty))
                    {
                        __result.Add(valueToApply, partySizeBonusText);
                    }
                    break;

                case IScope.Only_player_kingdom:
                    if (ScopeExtension.IsOnlyPlayerKingdom(mobileParty))
                    {
                        __result.Add(valueToApply, partySizeBonusText);
                    }
                    break;

                case IScope.Only_ennemies:
                    if (ScopeExtension.IsOnlyEnnemies(mobileParty))
                    {
                        __result.Add(valueToApply, partySizeBonusText);
                    }
                    break;
            }

            if (isPlayerImpacted && mobileParty.LeaderHero?.IsHumanPlayerCharacter == true)
            {
                // Update player's party
                __result.Add(valueToApply, partySizeBonusText);
            }
        }
    }
}