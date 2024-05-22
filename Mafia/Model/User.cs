namespace Mafia.Model;

public class User
{
    public required string Nick { get; set; }

    public bool IsSelected { get; set; } = true;

    public Dictionary<string, Dictionary<ulong, int>> Ratings { get; set; } = [];
    public Dictionary<string, int> Rating => Ratings.ToDictionary(kv => kv.Key, kv => kv.Value.Values.Sum());

    public override string ToString() => Nick;
}
