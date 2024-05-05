namespace Mafia.Model;

public class Action
{
    public required string Name { get; set; }
    public string[]? Conditions { get; set; }
    public required string[] Operations { get; set; }

    public required Execution Execution { get; set; }
}
