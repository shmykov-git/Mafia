using Mafia.Extensions;

namespace Mafia.Model;

public class Replay
{
    public string MapName { get; set; }
    public string MapVersion { get; set; }
    public string Language { get; set; }
    public (string nick, string role)[] Players { get; set; } = [];
    public (int who, int[] whom)[][] Selections { get; set; } = [];
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public override string ToString() => Selections.SelectMany(vv => vv.Select(v => $"({v.who}, [{v.whom.SJoin(", ")}])")).SJoin(", ");
}
