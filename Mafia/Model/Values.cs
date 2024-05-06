using Mafia.Executions;

namespace Mafia.Model;

public static class Values
{
    public static string[] KillOperations = [nameof(Operations.Kill), nameof(Operations.RoundKill), nameof(CityOperations.CityKill)];
    public static string[] HealOperations = [nameof(Operations.Heal)];
    public static string[] LockOperations = [nameof(Operations.Lock)];
    public static string[] CheckOperations = [nameof(Operations.Check)];
}
