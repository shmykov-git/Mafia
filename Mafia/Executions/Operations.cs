﻿using Mafia.Model;

namespace Mafia.Executions;

public delegate DailyNews Operation(State state, Player player);

public static class Operations
{
    private static DailyNews Select(string name, State state, Player player) => new DailyNews
    {
        Selects = [new Select { Operation = name, Who = player, Whom = state.Host.AskToSelect(state, player) }]
    };

    public static DailyNews Kill(State state, Player player) => Select(nameof(Kill), state, player);
    public static DailyNews Lock(State state, Player player) => Select(nameof(Lock), state, player);
    public static DailyNews Check(State state, Player player) => Select(nameof(Check), state, player);
    public static DailyNews Heal(State state, Player player) => Select(nameof(Heal), state, player);
    public static DailyNews Hello(State state, Player player) => new DailyNews();
    public static DailyNews RoundKill(State state, Player player) => Select(nameof(RoundKill), state, player);
}
