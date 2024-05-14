using System;
using Mafia.Executions;
using Mafia.Extensions;
using Mafia.Libraries;

namespace Mafia.Model;

/// <summary>
/// В первый день неизвестно, под каким игроком находится роль в Player, т.е. Player.User = null
/// по мере прохождения первой ночи роли могут определяться. 
/// Определение связи Player - User происходит по мере поступления информации и по мере необходимости ее знать
/// </summary>
public class DailyNews
{
    public List<Select>? Selects { get; set; }
    public List<SelectLock>? SelectLocks { get; set; }

    // <calculated>
    public Group[] KillGroups { get; set; } = [];
    public Player[] FactKills { get; set; } = [];
    // </calculated>

    public Player[] FactHeals => GetKills().Where(k => GetHeals().Contains(k) && !FactKills.Contains(k)).ToArray();

    public void Collect(DailyNews other)
    {
        if (other?.Selects != null)
        {
            if (Selects == null)
                Selects = [];

            Selects.AddRange(other.Selects);
        }

        if (other?.SelectLocks != null)
        {
            if (SelectLocks == null)
                SelectLocks = [];

            SelectLocks.AddRange(other.SelectLocks);
        }
    }

    public IEnumerable<Select> AllSelects() => Selects ?? [];
    public IEnumerable<SelectLock> AllSelectLocks() => SelectLocks ?? [];

    public Player[] GetLockedKillers() => AllSelectLocks()
        .Where(s => Values.NotLockedConditions.Contains(s.FailedCondition))         // putana
        .Where(s => !s.Who.Group.HasAnyOperation(Values.LockOperations))            // no putana here
        .Where(s => Values.KillOperations.Intersect(s.SkippedOperations).Any())     // killer
        .Select(l => l.Who).ToArray();

    public Player[] GetKills() => AllSelects().Where(s => Values.KillOperations.Contains(s.Operation)).SelectMany(s=>s.Whom).ToArray();
    public Player[] GetHeals() => AllSelects().Where(s => Values.HealOperations.Contains(s.Operation)).SelectMany(s => s.Whom).ToArray();
    public Player[] GetLocks() => AllSelects().Where(s => Values.LockOperations.Contains(s.Operation)).SelectMany(s => s.Whom).ToArray();

    public Select[] AllKnownLocks(State state) => GetKnownSelects(state, Values.LockOperations);
    public Select[] AllKnownKills(State state) => GetKnownSelects(state, Values.KillOperations);
    public Select[] AllKnownHeals(State state) => GetKnownSelects(state, Values.HealOperations);
    //public Select[] AllKnownChecks(State state) => GetKnownSelects(state, Values.CheckOperations);

    public void DoKnowAllWhom(State state) => AllSelects().ForEach(s => DoKnowWhom(state, s));

    private void DoKnowWhom(State state, Select select)
    {
        if (select.IsWhomKnown)
            return;

        select.Whom = select.UserWhom.Select(u => state.Players0.SingleOrDefault(p => p.User == u)).Where(p => p != null).ToArray()!;
    }

    private Select[] GetKnownSelects(State state, string[] operations)
    {
        var selects = AllSelects().Where(s => operations.Contains(s.Operation)).ToArray();
        selects.ForEach(s => DoKnowWhom(state, s));

        return selects;
    }
}
