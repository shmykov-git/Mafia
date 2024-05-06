using Mafia.Executions;

namespace Mafia.Model;

/// <summary>
/// todo:
/// </summary>
public class DailyNews
{
    public List<Select>? Selects { get; set; }

    public Player[] Locked { get; set; }
    public Player[] Healed { get; set; }
    public Player[] Killed { get; set; }
    public Player[] Checked { get; set; }

    public void Collect(DailyNews other)
    {
        if (other?.Selects == null) 
            return;

        if (Selects == null)
            Selects = [];

        Selects.AddRange(other.Selects);
    }

    public IEnumerable<Select> AllSelects() => Selects ?? [];


    public Select[] AllLocks() => GetSelects(Values.LockOperations);
    public Select[] AllKills() => GetSelects(Values.KillOperations);
    public Select[] AllHeals() => GetSelects(Values.HealOperations);
    public Select[] AllChecks() => GetSelects(Values.CheckOperations);

    private Select[] GetSelects(string[] operations) => AllSelects().Where(s => operations.Contains(s.Operation)).ToArray();
}
