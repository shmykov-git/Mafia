using Newtonsoft.Json;

namespace Mafia.Extensions;

public static class JsonExtensions
{
    public static T? FromJson<T>(this string json)
    {
        if (json == null)
            return default;

        if (typeof(T) == typeof(string))
            return (T)(object)json;

        return JsonConvert.DeserializeObject<T>(json);
    }

    public static string ToJson<T>(this T obj, Formatting formatting = Formatting.Indented)
    {
        return JsonConvert.SerializeObject(obj, formatting, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
        });
    }

    public static string ToJsonInLine<T>(this T obj)
    {
        return obj.ToJson(Formatting.None);
    }
}
