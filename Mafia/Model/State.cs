namespace Mafia.Model;

public class State
{
    public required IHost Host { get; set; }

    public int NumberOfDay { get; set; }
    public bool IsDay { get; set; }
    public bool IsNight => !IsDay;

    public required Player[] Players0 { get; set; }
    public required List<Player> Players { get; set; }
}
