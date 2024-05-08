using Mafia.Model;

namespace Mafia.Executions;

public delegate Task<bool> Condition(State state, Player player);

public static class Conditions
{
    public static async Task<bool> SelfSelected(State state, Player player) => state.IsSelfSelected(player);
    public static async Task<bool> Single(State state, Player player) => state.GetTeam(player).Count() == 1;
    public static async Task<bool> NotSingle(State state, Player player) => !(await Single(state, player));
    public static async Task<bool> SeniorRank(State state, Player player) => state.GetTeamSeniorRank(player) == player.Role.Rank;
    public static async Task<bool> Killed(State state, Player player) => state.LatestNews.Killed?.Contains(player) ?? false;
    public static async Task<bool> FirstDay(State state, Player player) => state.DayNumber == 1;
    public static async Task<bool> Skippable(State state, Player player) => !(await state.Host.AskToSkip(state, player));
    public static async Task<bool> Locked(State state, Player player) => state.LatestNews.Selects?.Any(s => s.Operation == nameof(Operations.Lock) && s.Whom.Contains(player)) ?? false;
    public static async Task<bool> NotLocked(State state, Player player) => !(await Locked(state, player));
}
