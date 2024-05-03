namespace Mafia.Models;

public class ActEvent
{
    public required Act Act { get; set; }
    public required Event[] Events { get; set; }
}
