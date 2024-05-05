namespace Mafia.Model;

public class Role
{
    public required string Name { get; set; }
    public int Rank { get; set; }
    public Action[]? Actions { get; set; }

    public IEnumerable<Action> AllActions() => Actions ?? new Action[0];
    public override string ToString() => $"{Name}{(Rank == 0 ? "" : Rank.ToString())}";
}
