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

    // <calculated>
    public Player[] FactKilled { get; set; } = [];
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
    public Player[] GetKills() => AllSelects().Where(s => Values.KillOperations.Contains(s.Operation)).SelectMany(s=>s.Whom).ToArray();


    public Select[] AllKnownLocks(State state) => GetKnownSelects(state, Values.LockOperations);
    public Select[] AllKnownKills(State state) => GetKnownSelects(state, Values.KillOperations);
    public Select[] AllKnownHeals(State state) => GetKnownSelects(state, Values.HealOperations);
    //public Select[] AllKnownChecks(State state) => GetKnownSelects(state, Values.CheckOperations);

    public void DoKnowAllWhom(State state) => AllSelects().ForEach(s => DoKnowWhom(state, s));

    private void DoKnowWhom(State state, Select select)
    {
        if (select.IsWhomUnknown)
            return;

        select.Whom = select.UserWhom.Select(u => state.Players0.Single(p => p.User == u)).ToArray();
    }

    private Select[] GetKnownSelects(State state, string[] operations)
    {
        var selects = AllSelects().Where(s => operations.Contains(s.Operation)).ToArray();
        selects.ForEach(s => DoKnowWhom(state, s));

        return selects;
    }
}
