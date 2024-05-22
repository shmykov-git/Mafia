using Mafia.Exceptions;
using Mafia.Extensions;
using Mafia.Model;
using Action = Mafia.Model.Action;

namespace Mafia.Hosts;

public abstract class ReplayHost : IHost
{
    protected readonly Replay replay;
    protected User[] users;
    private int nNews = 0;
    private int nSelect = 0;

    public ReplayHost(Replay replay)
    {
        this.replay = replay;
        users = replay.Players.Select(p => new User { Nick = p.nick }).ToArray();
    }

    protected virtual void Select(Player? who, Player[] whom, string operation) { }
    protected virtual void NotifyDailyNews(State state) { }

    private User[] DoSelect(State state, Player? player, string operation)
    {
        if (nNews != state.News.Count)
        {
            (nNews, nSelect) = (state.News.Count, 0);
            NotifyDailyNews(state);
        }

        var selection = replay.Selections[nNews - 1][nSelect++];

        var who = selection.who == -1 ? null : state.Players0[selection.who];
        var whom = selection.whom.Select(j => state.Players0[j]).ToArray();

        if (player != who)
            throw new InvalidReplayException();

        Select(who, whom, operation);

        return selection.whom.Select(j => users[j]).ToArray();
    }

    public virtual async Task<User[]> AskCityToSelect(State state, CityAction action, string operation) => DoSelect(state, null, operation);
    public virtual async Task<User[]> AskToSelect(State state, Player player, Action action, string operation) => DoSelect(state, player, operation);
    public virtual async Task<User[]> GetNeighbors(State state, Player player, Action action, string operation) => DoSelect(state, player, operation);

    public virtual string[] GetGameRoles() => replay.Players.Select(p => p.role).ToArray();
    public virtual User[] GetGameUsers() => users;
    public virtual async Task StartGame(State state)
    {
        if (state.City.Name != replay.MapName)
            throw new InvalidReplayException();

        if (state.City.Language != replay.Language)
            throw new InvalidReplayException();

        if (state.City.Version != replay.MapVersion)
            throw new InvalidReplayException();

        state.Players0.ForEach((p, i) => p.User = users[i]);
    }

    public virtual async Task NotifyCityAfterDay(State state) { }

    public virtual async Task NotifyCityAfterNight(State state) { }

    public virtual async Task NotifyGameEnd(State state, Group group) { }

    public virtual async Task NotifyNightStart(State state) { }
    public virtual async Task NotifyDayStart(State state) { }


    public virtual async Task<bool> IsGameEnd(State state) => false;
    public virtual async Task Hello(State state, Player player) { }

    public void ChangeSeed(int seed) => throw new NotSupportedException();
    public void RolledBack(State state) => throw new NotSupportedException();
}
