using Mafia.Executions;
using Mafia.Libraries;

namespace Mafia.Model;

/// <summary>
/// todo:
/// </summary>
public class DailyNews
{
    public List<Select>? Selects { get; set; }

    // <calculated>
    public Player[] Locked { get; set; } = [];
    public Player[] Healed { get; set; } = [];
    public Player[] Killed { get; set; } = [];
    public Player[] Checked { get; set; } = [];
    // </calculated>

    public void Collect(DailyNews other)
    {
        if (other?.Selects == null) 
            return;

        if (Selects == null)
            Selects = [];

        Selects.AddRange(other.Selects);
    }

    public IEnumerable<Select> AllSelects() => Selects ?? [];

    public Player[] GetKills() => GetWhom(Values.KillOperations);

    public Select[] AllLocks() => GetSelects(Values.LockOperations);
    public Select[] AllKills() => GetSelects(Values.KillOperations);
    public Select[] AllHeals() => GetSelects(Values.HealOperations);
    public Select[] AllChecks() => GetSelects(Values.CheckOperations);

    private Select[] GetSelects(string[] operations) => AllSelects().Where(s => operations.Contains(s.Operation)).ToArray();
    private Player[] GetWhom(string[] operations) => AllSelects().Where(s => operations.Contains(s.Operation)).SelectMany(s => s.Whom).ToArray();
}
