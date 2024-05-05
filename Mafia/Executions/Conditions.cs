using Mafia.Model;

namespace Mafia.Executions;

public delegate bool Condition(State state, Player player);

public static class Conditions
{
    private static Player[] GetPlayerTeam(ICollection<Player> players, Role[] roles) => players.Where(p => roles.Contains(p.Role)).ToArray();

    public static bool Single(State state, Player player) => GetPlayerTeam(state.Players, player.Group.Roles!).Count() == 1;
    public static bool NotSingle(State state, Player player) => !Single(state, player);
    public static bool SeniorRank(State state, Player player) => GetPlayerTeam(state.Players, player.Group.Roles!).Min(p => p.Role.Rank) == player.Role.Rank;
    public static bool Killed(State state, Player player) => !state.Players.Contains(player);
    public static bool FirstDay(State state, Player player) => state.NumberOfDay == 1;
    public static bool Skippable(State state, Player player) => state.Host.AskToSkip(state, player);
}
