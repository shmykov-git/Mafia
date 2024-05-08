using System.Reflection.Metadata.Ecma335;

namespace Mafia.Extensions;

public static class TypeExtensions
{
    public static bool IsTask(this Type type)
    {
        return type == typeof(Task) || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>));
    }
}
