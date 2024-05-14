namespace Mafia.Model;

public class SelectLock
{
    public required string Condition { get; set; }
    public required Player Who { get; set; }
    public required string[] Operations { get; set; }
}