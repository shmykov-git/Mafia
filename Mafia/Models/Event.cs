namespace Mafia.Models;

public class Event
{
    public string? group;
    public string? role;
    public required Command command;
    public Player[] selections;
}
