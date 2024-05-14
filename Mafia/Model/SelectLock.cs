using Mafia.Extensions;

namespace Mafia.Model;

public class SelectLock
{
    public required string FailedCondition { get; set; }
    public required Player Who { get; set; }
    public required string[] SkippedOperations { get; set; }

    public override string ToString() => $"{Who} failed {FailedCondition} to {SkippedOperations.SJoin(", ")}";
}