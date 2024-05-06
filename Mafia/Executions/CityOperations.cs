using Mafia.Model;

namespace Mafia.Executions;

public delegate OperationResult CityOperation(State state);

public static class CityOperations
{
    private static OperationResult CitySelect(string name, State state) => new OperationResult
    {
        Selects = [new Select { Operation = name, Who = null!, Whom = [state.Host.AskCityToSelect(state)] }]
    };

    public static OperationResult CityKill(State state) => CitySelect(nameof(CityKill), state);
}
