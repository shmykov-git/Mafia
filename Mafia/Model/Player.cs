namespace Mafia.Model;

public class Player
{
    public required User User { get; set; }
    public required Role Role { get; set; }
    public required Group Group { get; set; }

    public override string ToString() => $"{User.Nick}-{Role}";
}
