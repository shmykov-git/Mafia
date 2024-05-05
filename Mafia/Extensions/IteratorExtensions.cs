using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Mafia.Extensions;

public class IteratorIgnoreAttribute : Attribute { }

public static class IteratorExtensions
{
    public static IEnumerable<TItem> LazyIterateDeepLeft<TItem>(this object obj)
    {
        foreach ((var item, int _) in obj.LazyIterateDeepLeftLevel<TItem>())
            yield return item;
    }

    public static IEnumerable<(TItem item, int level)> LazyIterateDeepLeftLevel<TItem>(this object obj, int level = 0)
    {
        IEnumerable<TItem>? GetItems(PropertyInfo p) => (IEnumerable<TItem>?)p.GetValue(obj);
        var properties = obj.GetType().GetProperties()
            .Where(p => !p.CustomAttributes.Any(a => a.AttributeType == typeof(IteratorIgnoreAttribute)))
            .Where(p => p.PropertyType == typeof(TItem[]) 
                     || p.PropertyType == typeof(List<TItem>) 
                     || p.PropertyType == typeof(IEnumerable<TItem>) 
                     || p.PropertyType == typeof(ICollection<TItem>));

        foreach (var items in properties.Select(GetItems))
        {
            if (items != null)
                foreach (var item in items)
                {
                    foreach (var sub in LazyIterateDeepLeftLevel<TItem>(item!, level + 1))
                        yield return sub;

                    yield return (item, level);
                }
        }
    }
}
