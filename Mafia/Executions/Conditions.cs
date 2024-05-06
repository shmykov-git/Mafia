using Mafia.Model;

namespace Mafia.Executions;

public delegate bool Condition(State state, Player player);

public static class Conditions
{
    public static bool SelfSelected(State state, Player player) => state.IsSelfSelected(player);
    public static bool NotSelfSelected(State state, Player player) => !SelfSelected(state, player);
    public static bool Single(State state, Player player) => state.GetTeam(player).Count() == 1;
    public static bool NotSingle(State state, Player player) => !Single(state, player);
    public static bool SeniorRank(State state, Player player) => state.GetTeamSeniorRank(player) == player.Role.Rank;
    public static bool Killed(State state, Player player) => state.LatestNews.Killed?.Contains(player) ?? false;
    public static bool FirstDay(State state, Player player) => state.DayNumber == 1;
    public static bool Skippable(State state, Player player) => !state.Host.AskToSkip(state, player);
    public static bool Locked(State state, Player player) => state.LatestNews.Selects?.Any(s => s.Operation == nameof(Operations.Lock) && s.Whom.Contains(player)) ?? false;
    public static bool NotLocked(State state, Player player) => !Locked(state, player);
}
