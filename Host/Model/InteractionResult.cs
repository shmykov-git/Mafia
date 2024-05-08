using Mafia.Model;

namespace Host.Model;

public class InteractionResult
{
    public bool Skip { get; set; }
    public Player[] Selected { get; set; }
}