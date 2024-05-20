using Mafia.Extensions;
using Mafia.Libraries;

namespace Host.Libraries;

public static class Stores
{
    public static Task SetDataByKey<TValue>(string key, TValue value) => Runs.DoPersist(async () => Preferences.Default.Set(key, value.ToJson()));
    public static Task<TValue> GetDataByKey<TValue>(string key) where TValue : new() => Runs.DoPersist(async () => Preferences.Default.Get<string?>(key, null).FromJson<TValue>() ?? new TValue());

    public static Task SetSecureDataByKey<TValue>(string key, TValue value) => Runs.DoPersist(() => SecureStorage.Default.SetAsync(key, value.ToJson()));
    public static Task<TValue> GetSecureDataByKey<TValue>(string key) where TValue : new() => Runs.DoPersist(async () => (await SecureStorage.Default.GetAsync(key)).FromJson<TValue>() ?? new TValue());
}
