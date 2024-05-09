using Mafia.Model;

namespace Host.Model;

public class Interaction
{
    public required string Name { get; set; }
    public string[]? Args { get; set; }
    public required State State { get; set; }
    public Player? Player { get; set; }
    public Player[] Except { get; set; } = [];
    public Player[] Killed { get; set; } = [];
    public (int from, int to) Selection { get; set; } = (0, 0);
    
    public bool IsSkippable => Selection.from == 0;
    public bool WithCity => Player == null;
    public bool WithPlayer => Player != null;
}
