using System;
using System.ComponentModel;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;

namespace PartySizeReunited.Models
{
    public class ScopeExtension
    {
        public Scope Scope { get; }

        public ScopeExtension(Scope scope)
        {
            Scope = scope;
        }

        public override string ToString()
        {
            var field = Scope.GetType().GetField(Scope.ToString());
            var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? Scope.ToString();
        }

        public static bool IsInScope(Scope scope, PartyBase? party)
        {
            return party != null && Matches(
                scope,
                party.Owner?.IsHumanPlayerCharacter == true,
                party.Owner?.Clan);
        }

        public static bool IsInScope(Scope scope, MobileParty? party)
        {
            return party != null && Matches(
                scope,
                party.IsMainParty,
                party.ActualClan);
        }

        public static bool IsInScope(Scope scope, Settlement? settlement)
        {
            return settlement != null && Matches(
                scope,
                settlement.Owner?.IsHumanPlayerCharacter == true,
                settlement.Owner?.Clan);
        }

        private static bool Matches(
            Scope scope,
            bool isPlayer,
            Clan? entityClan)
        {
            bool includePlayer = SubModule.PartySizeReunitedOptions.IsPlayerPartyImpacted;
            var playerClan = Hero.MainHero?.Clan;
            var playerKingdom = playerClan?.Kingdom;
            var entityKingdom = entityClan?.Kingdom;
            bool isPlayerClan = entityClan != null && playerClan != null && entityClan == playerClan;
            bool isPlayerKingdom = HaveSameKingdomName(entityKingdom, playerKingdom);
            bool isEnemy = !isPlayerClan && !isPlayerKingdom;

            return scope switch
            {
                Scope.Everyone => !isPlayer || includePlayer,
                Scope.OnlyPlayer => isPlayer && includePlayer,
                Scope.OnlyPlayerClan => isPlayer ? includePlayer : isPlayerClan,
                Scope.OnlyPlayerKingdom => isPlayer ? includePlayer : isPlayerKingdom,
                Scope.OnlyEnnemies => isPlayer ? includePlayer : isEnemy,
                _ => false
            };
        }

        private static bool HaveSameKingdomName(Kingdom? first, Kingdom? second)
        {
            return first != null &&
                   second != null &&
                   string.Equals(first.Name?.ToString(), second.Name?.ToString(), StringComparison.Ordinal);
        }

    }

    public enum Scope
    {
        [Description("Everyone (Except player)")]
        Everyone,

        [Description("Only player")]
        OnlyPlayer,

        [Description("Only player clan")]
        OnlyPlayerClan,

        [Description("Only player kingdom")]
        OnlyPlayerKingdom,

        [Description("Only enemies")]
        OnlyEnnemies
    }
}
