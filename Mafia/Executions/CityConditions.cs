using Mafia.Model;

namespace Mafia.Executions;

public delegate Task<bool> CityCondition(State state);

public static class CityConditions
{
    public static async Task<bool> CitySkippable(State state) => !(await state.Host.AskCityToSkip(state));
}
