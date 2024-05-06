using Mafia.Executions;

namespace Mafia.Model;

public class DailyNews
{
    public List<Select>? Selects { get; set; }

    public Player[] Locked { get; set; }
    public Player[] Healed { get; set; }
    public Player[] Killed { get; set; }

    public void Collect(DailyNews other)
    {
        if (other?.Selects == null) 
            return;

        if (Selects == null)
            Selects = [];

        Selects.AddRange(other.Selects);
    }

    public IEnumerable<Select> AllSelects() => Selects ?? [];


    public Select[] AllKills() => GetSelects(Values.KillOperations);
    public Select[] AllHeals() => GetSelects(Values.HealOperations);
    public Select[] AllLocks() => GetSelects(Values.HealOperations);

    private Select[] GetSelects(string[] operations) => AllSelects().Where(s => operations.Contains(s.Operation)).ToArray();
}
