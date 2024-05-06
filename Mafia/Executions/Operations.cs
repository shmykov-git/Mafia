using Mafia.Model;

namespace Mafia.Executions;

public delegate OperationResult Operation(State state, Player player);

public static class Operations
{
    private static OperationResult Select(string name, State state, Player player) => new OperationResult
    {
        Selects = [new Select { Operation = name, Who = player, Whom = [state.Host.AskToSelect(state, player)] }]
    };

    private static OperationResult SelectNotSelf(string name, State state, Player player) => new OperationResult
    {
        Selects = [new Select { Operation = name, Who = player, Whom = [state.Host.AskToSelectNotSelf(state, player)] }]
    };

    public static OperationResult Kill(State state, Player player) => Select(nameof(Kill), state, player);
    public static OperationResult Lock(State state, Player player) => Select(nameof(Lock), state, player);
    public static OperationResult Check(State state, Player player) => Select(nameof(Check), state, player);
    public static OperationResult Heal(State state, Player player) => Select(nameof(Heal), state, player);
    public static OperationResult HealNotSelf(State state, Player player) => SelectNotSelf(nameof(Heal), state, player);
    public static OperationResult Hello(State state, Player player) => new OperationResult();
    public static OperationResult RoundKill(State state, Player player) => Select(nameof(RoundKill), state, player);
}
