using Mafia.Model;

namespace Mafia.Executions;

public delegate bool CityCondition(State state);

public static class CityConditions
{
    public static bool CitySkippable(State state) => !state.Host.AskCityToSkip(state);
}
