namespace Mafia.Model;

public class OperationResult
{
    public List<Select>? Selects { get; set; }

    public void Collect(OperationResult other)
    {
        if (other?.Selects == null) 
            return;

        if (Selects == null)
            Selects = [];

        Selects.AddRange(other.Selects);
    }

    public IEnumerable<Select> AllSelects() => Selects ?? [];
    public Player[] AllKills() => AllSelects().Where(s => Values.KillOperations.Contains(s.Operation)).SelectMany(s => s.Whom).ToArray();
}
