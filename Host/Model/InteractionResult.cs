using Mafia.Model;

namespace Host.Model;

public class InteractionResult
{
    public Player[] Selected { get; set; } = [];
    public bool IsSkipped => Selected.Length == 0;
}