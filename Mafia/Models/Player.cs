namespace Mafia.Models;

public class Player
{
    public required User User { get; set; }
    public required string Role { get; set; }
    public SelectAct? SelectAct { get; set; }
    public required Group ParticipantGroup { get; set; }

    public override string ToString() => $"{User.Nick}-{Role}";
}
