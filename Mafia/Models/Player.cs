namespace Mafia.Models;

public class Player
{
    public required User User { get; set; }
    public required string Role { get; set; }
    public Group? Group { get; set; }
    public Act? Act { get; set; }

    public override string ToString() => $"{User.Nick}-{Role}";
}
