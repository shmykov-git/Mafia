using System.Xml.Linq;
using Mafia.Model;

namespace Mafia.Executions;

public delegate Task<DailyNews> Operation(State state, Player player);

public static class Operations
{
    private static async Task<DailyNews> Select(string name, State state, Player player) => new DailyNews
    {
        Selects = [new Select { Operation = name, Who = player, Whom = await state.Host.AskToSelect(state, player) }]
    };

    public static Task<DailyNews> Kill(State state, Player player) => Select(nameof(Kill), state, player);
    public static Task<DailyNews> Lock(State state, Player player) => Select(nameof(Lock), state, player);
    public static Task<DailyNews> Check(State state, Player player) => Select(nameof(Check), state, player);
    public static Task<DailyNews> Heal(State state, Player player) => Select(nameof(Heal), state, player);
    public static DailyNews Hello(State state, Player player) => new DailyNews();

    public static async Task<DailyNews> RoundKill(State state, Player player) => new DailyNews
    {
        Selects = [new Select { Operation = nameof(RoundKill), Who = player, Whom = await state.Host.GetNeighbors(state, player) }]
    };
}
