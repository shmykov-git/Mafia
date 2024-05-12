using System.Linq;
using Mafia.Model;

namespace Mafia.Executions;

public delegate Task<bool> Condition(State state, Player player);

public static class Conditions
{
    private static bool Locked(State state, Player player) => state.LatestNews.Selects?.Any(s => s.Operation == nameof(Operations.Lock) && s.UserWhom.Contains(player.User)) ?? false;
    private static bool ActiveKilled(State state, Player player) => state.GetLatestFactKills().Contains(player);


    public static bool SelfSelected(State state, Player player) => state.IsSelfSelected(player);
    public static bool Single(State state, Player player) => state.GetTeam(player).Count() == 1;
    public static bool NotSingle(State state, Player player) => !Single(state, player);
    public static bool SeniorRank(State state, Player player) => state.GetTeamSeniorRank(player) == player.Role.Rank;
    public static bool Killed(State state, Player player) => state.LatestNews.FactKilled?.Contains(player) ?? false;
    public static bool FirstDay(State state, Player player) => state.DayNumber == 1;
    public static bool Skippable(State state, Player player) => true;


    // <Active>
    public static bool NotKilledAlone(State state, Player player) => !ActiveKilled(state, player) || state.GetTeamOthers(player).Any();
    public static bool NotLocked(State state, Player player) => !Locked(state, player);
    // </Active>
}
