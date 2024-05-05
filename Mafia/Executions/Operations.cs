using Mafia.Model;

namespace Mafia.Executions;

public delegate OperationResult Operation(State state, Player player);

public static class Operations
{
    public static OperationResult Kill(State state, Player player)
    {
        var kill = state.Host.AskToSelect(state, player);

        return new OperationResult { Kills = [kill] };
    }

    public static OperationResult RankKill(State state, Player player)
    {
        throw new NotImplementedException();
    }

    public static OperationResult Lock(State state, Player player)
    {
        throw new NotImplementedException();
    }

    public static OperationResult Check(State state, Player player)
    {
        throw new NotImplementedException();
    }

    public static OperationResult Heal(State state, Player player)
    {
        throw new NotImplementedException();
    }

    public static OperationResult Hello(State state, Player player)
    {
        throw new NotImplementedException();
    }

    public static OperationResult RoundKill(State state, Player player)
    {
        throw new NotImplementedException();
    }
}
