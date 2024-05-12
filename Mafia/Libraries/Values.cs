using Mafia.Executions;

namespace Mafia.Libraries;

public static class Values
{
    public static string[] KillOperations = [nameof(Operations.Kill), nameof(Operations.RoundKill), nameof(CityOperations.CityKill)];
    public static string[] HealOperations = [nameof(Operations.Heal)];
    public static string[] LockOperations = [nameof(Operations.Lock)];
    public static string[] CheckOperations = [nameof(Operations.Check)];

    public static string[] OnDeathConditions = [nameof(Conditions.Killed)];
    
    /// <summary>
    /// Can be checked during current night (or day). Example: Prostitute (Putana) visited Maniac, Maniac cannot kill same night right after the visit
    /// </summary>
    public static string[] ActiveConditions = [nameof(Conditions.Locked), nameof(Conditions.NotLocked)];
    public static string[] SkippableConditions = [nameof(CityConditions.CitySkippable), nameof(Conditions.Skippable)];
}
