namespace Mafia.Models;

public class EventInfo
{
    public string[]? roles;
    public Command command;
    public Act? act;
    public bool skippable;

    public bool IsGroupOrPersonEvent => !IsCityEvent;
    public bool IsCityEvent => roles == null;
    public bool HasSelection => command == Command.Select;
}
