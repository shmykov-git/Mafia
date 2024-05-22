using System.Text.Json.Serialization;
using Mafia.Extensions;

namespace Mafia.Model;

public class Replay
{
    public ulong Id => new IEnumerable<ulong>[]
    {
        [MapName.GetULongHash(), MapVersion.GetULongHash()],
        Players.Select(p=>p.nick.GetULongHash()),
        Players.Select(p=>p.role.GetULongHash()),
        Selections.SelectMany(ss=>ss).Select(s=>s.who.GetULongHash()),
        Selections.SelectMany(ss=>ss).SelectMany(ss=>ss.whom).Select(v=>v.GetULongHash())
    }.GetULongHash();

    public string MapName { get; set; }
    public string MapVersion { get; set; }
    public string Language { get; set; }
    public (string nick, string role)[] Players { get; set; } = [];
    public (int who, int[] whom)[][] Selections { get; set; } = [];

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public override string ToString() => Selections.SelectMany(vv => vv.Select(v => $"({v.who}, [{v.whom.SJoin(", ")}])")).SJoin(", ");
}
