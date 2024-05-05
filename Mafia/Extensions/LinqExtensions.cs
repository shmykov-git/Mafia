namespace Mafia.Extensions;

public static class LinqExtensions
{
    public static IEnumerable<T> TakeWhileNot<T>(this IEnumerable<T> values, Func<T, bool> condition) => values.TakeWhile(v => !condition(v)).ToList();
    public static IEnumerable<T> SkipWhileNot<T>(this IEnumerable<T> values, Func<T, bool> condition) => values.SkipWhile(v => !condition(v)).ToList();

    public static void ForEach<T>(this IEnumerable<T> values, Action<T, int> action)
    {
        var en = values.GetEnumerator();
        var i = 0;

        while (en.MoveNext())
            action(en.Current, i++);
    }

    public static void ForEach<T>(this IEnumerable<T> values, Action<T> action)
    {
        var en = values.GetEnumerator();

        while (en.MoveNext())
            action(en.Current);
    }
}
