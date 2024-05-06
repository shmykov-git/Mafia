namespace Mafia.Model;

public class Player
{
    public required User User { get; set; }
    public required Role Role { get; set; }
    public required Group Group { get; set; }

    public bool Is(string role) => Role.Name == role;
    public override string ToString() => $"{Group.Name}-{User.Nick}-{Role}";
}
