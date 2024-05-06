using Mafia.Executions;
using Mafia.Extensions;
using Mafia.Model;

namespace Mafia;

public class Game 
{
    private readonly City city;
    private readonly IHost host;
    private State state;

    public Game(City city, IHost host)
    {
        this.city = city;
        this.host = host;
        InitCity();
    }

    private void InitCity()
    {
        city.DayActions.ForEach(cityAction =>
        {
            var conditions = cityAction.Conditions?.Select(name => typeof(CityConditions).GetMethods().First(m => m.Name == name))
                .ToDictionary(m => m.Name, m => (CityCondition)((s) => (bool)m.Invoke(null, new object[] { s })));

            var operations = cityAction.Operations.Select(name => typeof(CityOperations).GetMethods().First(m => m.Name == name))
                .ToDictionary(m => m.Name, m => (CityOperation)((s) => (DailyNews)m.Invoke(null, new object[] { s })));

            cityAction.Execution = new CityExecution { Conditions = conditions, Operations = operations };
        });

        city.AllActions().ForEach(action =>
        {
            var conditions = action.Conditions?.Select(name => typeof(Conditions).GetMethods().First(m => m.Name == name))
                .ToDictionary(m => m.Name, m => (Condition)((s, p) => (bool)m.Invoke(null, new object[] { s, p })));

            var operations = action.Operations.Select(name => typeof(Operations).GetMethods().First(m => m.Name == name))
                .ToDictionary(m => m.Name, m => (Operation)((s, p) => (DailyNews)m.Invoke(null, new object[] { s, p })));

            action.Execution = new Execution { Conditions = conditions, Operations = operations };
        });
    }

    private void PlayDay()
    {
        var result = new DailyNews();
        state.News.Add(result);

        foreach (var action in city.DayActions)
        {
            if (action.CheckConditions(state))
                result.Collect(action.DoOperations(state));
        }
    }

    private void PlayNight()
    {
        var result = new DailyNews();
        state.News.Add(result);

        foreach (var group in city.NightEvents.Select(groupName => city.GetGroup(groupName)))
        {
            foreach (var player in state.GetGroupPlayers(group))
            {
                foreach (var action in player.Role.AllActions())
                {
                    if (action.CheckConditions(state, player))
                        result.Collect(action.DoOperations(state, player));
                }
            }
        }
    }

    private void ApplyNightKills()
    {
        if (state.DayNumber == 1)
            return;

        var locks = state.LatestNews.AllLocks();

        var kills = state.LatestNews.AllKills()
            .Where(s=>!locks.Any(l=>l.Whom.Contains(s.Who)))
            .SelectMany(k=>k.Whom)
            .GroupBy(v=>v).Select(gv=>(p: gv.Key, c:gv.Count())).ToArray();

        var heals = state.LatestNews.AllHeals()
            .Where(s => !locks.Any(l => l.Whom.Contains(s.Who)))
            .SelectMany(k => k.Whom)
            .GroupBy(v => v).Select(gv => (p: gv.Key, c: gv.Count())).ToArray();

        var factKills = kills.Where(k => heals.FirstOrDefault(h => h.p == k.p).c < k.c).Select(k => k.p).ToArray();

        state.LatestNews.Locked = locks.SelectMany(l => l.Whom).ToArray();
        state.LatestNews.Healed = heals.Select(v=>v.p).ToArray();
        state.LatestNews.Killed = factKills;
        ApplyKills();
    }

    private void ApplyDayKills()
    {
        state.LatestNews.Killed = state.LatestNews.AllKills().SelectMany(l => l.Whom).ToArray();
        ApplyKills();
    }

    private void ApplyKills()
    {
        foreach(var player in state.LatestNews.Killed)
            state.Players.Remove(player);
    }

    private bool IsGameEnd() => state.DayNumber == 3;

    public void Start()
    {
        var players0 = host.GetPlayers();
        
        state = new State 
        { 
            Host = host, 
            City = city,
            Players0 = players0, 
            Players = players0.ToList(), 
            IsDay = true, 
            DayNumber = 1, 
            News = new() 
        };

        while (true)
        {
            state.IsDay = true;
            ApplyNightKills();
            host.NotifyCityAfterNight(state);
            if (IsGameEnd())
            {
                host.NotifyGameEnd(state);
                break;
            }
            PlayDay();
            ApplyDayKills();
            host.NotifyCityAfterDay(state);
            if (IsGameEnd())
            {
                host.NotifyGameEnd(state);
                break;
            }
            state.IsNight = true;
            PlayNight();

            state.DayNumber++;
        }
    }
}
