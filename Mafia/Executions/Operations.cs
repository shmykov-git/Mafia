using Mafia.Model;
using Action = Mafia.Model.Action;

namespace Mafia.Executions;

public delegate Task<DailyNews> Operation(State state, Player player, Action action);

public static class Operations
{
    private static async Task<DailyNews> Select(State state, Player player, Action action, string operation)
    {
        var dailyNews = new DailyNews
        {
            Selects = [new Select { Operation = operation, Who = player, UserWhom = await state.Host.AskToSelect(state, player, action, operation) }]
        };

        dailyNews.DoKnowAllWhom(state);

        return dailyNews;
    }

    public static Task<DailyNews> Kill(State state, Player player, Action action) => Select(state, player, action, nameof(Kill));
    public static Task<DailyNews> Lock(State state, Player player, Action action) => Select(state, player, action, nameof(Lock));
    public static Task<DailyNews> Check(State state, Player player, Action action) => Select(state, player, action, nameof(Check));
    public static Task<DailyNews> Heal(State state, Player player, Action action) => Select(state, player, action, nameof(Heal));
    public static async Task<DailyNews> Hello(State state, Player player, Action action) { await state.Host.Hello(state, player); return new DailyNews(); }

    public static async Task<DailyNews> RoundKill(State state, Player player, Action action)
    {
        var dailyNews = new DailyNews
        {
            Selects = [new Select { Operation = nameof(RoundKill), Who = player, UserWhom = await state.Host.GetNeighbors(state, player, action, nameof(RoundKill)) }]
        };

        dailyNews.DoKnowAllWhom(state);

        return dailyNews;
    }
}
