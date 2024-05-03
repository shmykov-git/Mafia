namespace Mafia.Models;

public class EventInfo
{
    public string[]? roles;
    public Command command;
    public Act? act;
    public Player[][]? players;
    public bool skippable;

    public Player[]? mainPlayers => players == null ? null : players[0];
    public bool IsPersonEvent => !IsCityEvent;
    public bool IsCityEvent => roles == null;
    public bool HasSelection => command == Command.Select;
}
