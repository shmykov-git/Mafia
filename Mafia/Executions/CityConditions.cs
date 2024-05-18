using Mafia.Model;

namespace Mafia.Executions;

public delegate Task<bool> CityCondition(State state);

public static class CityConditions
{
    public static bool CitySkippable(State state) => true;
    public static bool FirstDay(State state) => state.IsFirstDay;
    public static bool NotFirstDay(State state) => !state.IsFirstDay;
}
