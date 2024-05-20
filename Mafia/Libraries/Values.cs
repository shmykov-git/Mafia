using Mafia.Executions;

namespace Mafia.Libraries;

public static class Values
{
    public static string[] KillOperations = [nameof(Operations.Kill), nameof(Operations.RoundKill), nameof(CityOperations.CityKill)];
    public static string[] HealOperations = [nameof(Operations.Heal)];
    public static string[] LockOperations = [nameof(Operations.Lock)];
    public static string[] CheckOperations = [nameof(Operations.Check)];
    public static string[] UnwantedOperations = [nameof(Operations.Kill), nameof(Operations.Lock), nameof(Operations.Check)];

    public static string[] OnDeathConditions = [nameof(Conditions.Killed)];

    public static string[] NotLockedConditions = [nameof(Conditions.NotLocked)];
    public static string[] RankConditions = [nameof(Conditions.SeniorRank)];
    public static string[] ImmunityConditions = [nameof(CityOperations.CityImmunity)];
    
    /// <summary>
    /// Can be checked during current night (or day). Example: Prostitute (Putana) visited Maniac, Maniac cannot kill same night right after the visit
    /// </summary>
    public static string[] ActiveConditions = [nameof(Conditions.NotLocked), nameof(Conditions.NotKilledAlone)];
}
