using Mafia.Extensions;

namespace Mafia.Model;

public class Select
{
    public required string Operation { get; set; }
    public required Player Who { get; set; }
    public Player[] Whom { get; set; } = [];
    public required User[] UserWhom { get; set; }

    public bool IsWhomUnknown => UserWhom.Length == Whom.Length;
    public bool IsCity => Who == null;

    public override string ToString() => $"{Who} {Operation} {(IsWhomUnknown ? UserWhom.Select(u=>$"{u.Nick}-?").SJoin(", ") : Whom.SJoin(", "))}";
}
