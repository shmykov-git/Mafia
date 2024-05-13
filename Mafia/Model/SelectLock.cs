namespace Mafia.Model;

public class SelectLock
{
    public required string Condition { get; set; }
    public required Player Who { get; set; }
}