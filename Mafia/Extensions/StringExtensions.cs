﻿using System.Text.RegularExpressions;

namespace Mafia.Extensions;

public static class StringExtensions
{
    public static bool HasText(this string? text) => !string.IsNullOrWhiteSpace(text);

    public static string SJoin<T>(this IEnumerable<T> values, string del) => string.Join(del, values);

    public static (string, string?) ToPair(this IEnumerable<string> values)
    {
        return (values.First(), values.Skip(1).FirstOrDefault());
    }
}
