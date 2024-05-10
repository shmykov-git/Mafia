using System.Collections.ObjectModel;

namespace Host.Extensions;

public static class LinqExtensions
{
    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> values) => new ObservableCollection<T>(values);
}
