using Mafia.Model;

namespace Mafia.Executions;

public delegate DailyNews CityOperation(State state);

public static class CityOperations
{
    private static DailyNews CitySelect(string name, State state) => new DailyNews
    {
        Selects = [new Select { Operation = name, Who = null!, Whom = [state.Host.AskCityToSelect(state)] }]
    };

    public static DailyNews CityKill(State state) => CitySelect(nameof(CityKill), state);
}
