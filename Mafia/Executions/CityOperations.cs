using Mafia.Model;

namespace Mafia.Executions;

public delegate Task<DailyNews> CityOperation(State state);

public static class CityOperations
{
    private static async Task<DailyNews> CitySelect(string name, State state) => new DailyNews
    {
        Selects = [new Select { Operation = name, Who = null!, Whom = [await state.Host.AskCityToSelect(state)] }]
    };

    public static Task<DailyNews> CityKill(State state) => CitySelect(nameof(CityKill), state);
}
