namespace tmp.Models;

public class ActEvent
{
    public required string Role { get; set; }
    public required Event[] Events { get; set; }
}
