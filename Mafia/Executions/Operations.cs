using Mafia.Model;
using Action = Mafia.Model.Action;

namespace Mafia.Executions;

public delegate Task<DailyNews> Operation(State state, Player player, Action action);

public static class Operations
{
    private static async Task<DailyNews> Select(string name, State state, Player player, Action action) => new DailyNews
    {
        Selects = [new Select { Operation = name, Who = player, Whom = await state.Host.AskToSelect(state, player, action) }]
    };

    public static Task<DailyNews> Kill(State state, Player player, Action action) => Select(nameof(Kill), state, player, action);
    public static Task<DailyNews> Lock(State state, Player player, Action action) => Select(nameof(Lock), state, player, action);
    public static Task<DailyNews> Check(State state, Player player, Action action) => Select(nameof(Check), state, player, action);
    public static Task<DailyNews> Heal(State state, Player player, Action action) => Select(nameof(Heal), state, player, action);
    public static DailyNews Hello(State state, Player player, Action action) => new DailyNews();

    public static async Task<DailyNews> RoundKill(State state, Player player, Action action) => new DailyNews
    {
        Selects = [new Select { Operation = nameof(RoundKill), Who = player, Whom = await state.Host.GetNeighbors(state, player, action) }]
    };
}
