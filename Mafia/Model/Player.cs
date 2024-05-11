namespace Mafia.Model;

public class Player
{
    public required string Id { get; set; }
    public User? User { get; set; }
    public required Role Role { get; set; }
    public required Group Group { get; set; }
    public required Group TopGroup { get; set; }

    public bool Is(string role) => Role.Name == role;

    public override string ToString() => $"{User?.Nick??"Unknown"}-{Role}";
    //public override string ToString() => Group.AllRoles().Count() == 1 
    //    ? $"{User.Nick}-{Role}" 
    //    : $"{User.Nick}-{Role} ({Group.Name})";
}
