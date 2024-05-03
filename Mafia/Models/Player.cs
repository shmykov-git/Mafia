namespace Mafia.Models;

public class Player
{
    public User User { get; set; }
    public string Role { get; set; }
    public int? Day { get; set; }
}
