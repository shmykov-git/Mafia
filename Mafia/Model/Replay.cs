namespace Mafia.Model;

public class Replay
{
    public required string MapName { get; set; }
    public required string MapVersion { get; set; }
    public (string nick, string role)[] Players { get; set; } = [];
    public (int, int[])[][] Selections { get; set; } = [];
}
