using System.Text.RegularExpressions;

namespace Mafia.Extensions;

internal static class StringExtensions
{
    public static bool HasText(this string text) => !string.IsNullOrWhiteSpace(text);
    public static string SJoin(this IEnumerable<string> values, string del) => string.Join(del, values);

    public static (string, string?) ToPair(this IEnumerable<string> values)
    {
        return (values.First(), values.Skip(1).FirstOrDefault());
    }
}
