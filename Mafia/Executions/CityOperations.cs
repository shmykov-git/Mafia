using Mafia.Model;
using Action = Mafia.Model.Action;

namespace Mafia.Executions;

public delegate Task<DailyNews> CityOperation(State state, CityAction action);

public static class CityOperations
{
    private static async Task<DailyNews> CitySelect(string name, State state, CityAction action) => new DailyNews
    {
        Selects = [new Select { Operation = name, Who = null!, Whom = await state.Host.AskCityToSelect(state, action) }]
    };

    public static Task<DailyNews> CityKill(State state, CityAction action) => CitySelect(nameof(CityKill), state, action);
}
