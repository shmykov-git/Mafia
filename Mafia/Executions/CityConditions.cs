using Mafia.Model;

namespace Mafia.Executions;

public delegate Task<bool> CityCondition(State state);

public static class CityConditions
{
    public static bool CitySkippable(State state) => true;
}
