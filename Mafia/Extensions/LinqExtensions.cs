using System.Linq;
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

    public static async Task<bool> AnyAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, Task<bool>> predicate)
    {
        foreach (var item in source)
            if (await predicate(item))
                return true;

        return false;
    }

    public static async Task<bool> AllAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, Task<bool>> predicate)
    {
        foreach (var item in source)
            if (! (await predicate(item)))
                return false;

        return true;
    }

    public static async IAsyncEnumerable<TSource> WhereAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, Task<bool>> predicate)
    {
        foreach (var item in source)
            if (await predicate(item))
                yield return item;
    }

}
