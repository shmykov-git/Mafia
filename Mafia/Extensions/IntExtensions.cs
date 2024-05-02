namespace Mafia.Extensions;

internal static class IntExtensions
{
    public static bool Between(this int value, int a, int b) => a <= value && value <= b;
}
