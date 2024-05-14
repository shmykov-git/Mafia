using System.Linq;
using System.Numerics;
using System.Reflection;
using Mafia.Exceptions;
using Mafia.Executions;
using Mafia.Extensions;
using Mafia.Libraries;
using Mafia.Model;

namespace Mafia;

public class Game 
{
    private readonly City city;

    private readonly Func<IHost> hostFactory;
    private IHost _host;
    private IHost host => _host ??= hostFactory();

    private State state;
    public bool IsActive => state?.IsActive ?? false;
    public bool Stopping => state == null || state.Stopping;

    public Game(City city, Func<IHost> hostFactory)
    {
        this.city = city;
        this.hostFactory = hostFactory;
        InitCity();
    }

    private void InitCity()
    {
        CityCondition GetCityCondition(MethodInfo m) => m.ReturnParameter.ParameterType.IsTask()
            ? (s => (Task<bool>)m.Invoke(null, [s])!)
            : (s => Task.FromResult((bool)m.Invoke(null, [s])!));

        CityOperation GetCityOperation(MethodInfo m) => m.ReturnParameter.ParameterType.IsTask()
            ? ((s, a) => (Task<DailyNews>)m.Invoke(null, [s, a])!)
            : ((s, a) => Task.FromResult((DailyNews)m.Invoke(null, [s, a])!));

        Condition GetCondition(MethodInfo m) => m.ReturnParameter.ParameterType.IsTask()
            ? ((s, p) => (Task<bool>)m.Invoke(null, [s, p])!)
            : ((s, p) => Task.FromResult((bool)m.Invoke(null, [s, p])!));

        Operation GetOperation(MethodInfo m) => m.ReturnParameter.ParameterType.IsTask()
            ? ((s, p, a) => (Task<DailyNews>)m.Invoke(null, [s, p, a])!)
            : ((s, p, a) => Task.FromResult((DailyNews)m.Invoke(null, [s, p, a])!));

        city.DayActions.ForEach(cityAction =>
        {
            var conditions = cityAction.Conditions?.Select(name => typeof(CityConditions).GetMethods().Single(m => m.Name == name))
                .ToDictionary(m => m.Name, m => GetCityCondition(m));

            var operations = cityAction.Operations.Select(name => typeof(CityOperations).GetMethods().Single(m => m.Name == name))
                .ToDictionary(m => m.Name, m => GetCityOperation(m));

            cityAction.Execution = new CityExecution { Conditions = conditions, Operations = operations };
        });

        city.AllActions().ForEach(action =>
        {
            var conditions = action.Conditions?.Select(name => typeof(Conditions).GetMethods().Single(m => m.Name == name))
                .ToDictionary(m => m.Name, m => GetCondition(m));

            var operations = action.Operations.Select(name => typeof(Operations).GetMethods().Single(m => m.Name == name))
                .ToDictionary(m => m.Name, m => GetOperation(m));

            action.Execution = new Execution { Conditions = conditions, Operations = operations };
        });
    }

    private async Task PlayDay()
    {
        var news = new DailyNews();
        state.News.Add(news);

        foreach (var action in city.DayActions)
        {
            if (await action.CheckConditions(state))
                news.Collect(await action.DoOperations(state));
        }

        state.DoKnowAllLatestWhom();
    }

    private async Task PlayNight()
    {
        var news = new DailyNews();
        state.News.Add(news);

        foreach (var group in city.NightEvents.Select(city.GetGroup))
        {
            foreach (var player in state.GetGroupActivePlayers(group))
            {
                foreach (var action in player.Role.AllActions())
                {
                    if (await action.CheckConditions(state, player))
                        news.Collect(await action.DoOperations(state, player));
                    else
                        news.Collect(await action.GetBlockNews(state, player));
                }
            }
        }

        state.DoKnowAllLatestWhom();
    }

    /// <summary>
    /// todo: config
    /// </summary>
    private void CalcNightKills()
    {
        state.LatestNews.FactKills = state.GetLatestFactKills();
    }

    private void CalcDayKills()
    {
        state.LatestNews.FactKills = state.GetLatestFactKills();
    }

    private void ApplyKills()
    {
        foreach(var player in state.LatestNews.FactKills)
            state.Players.Remove(player);
    }

    private async Task PlayOnDeathKills()
    {
        var stack = new Stack<Player>();
        state.LatestNews.FactKills.ForEach(stack.Push);
        // нужно определить роль убитого
        var dailyNews = new DailyNews();

        while(stack.TryPop(out var kill))
        {
            foreach(var action in kill.Role.AllActions().Where(a=>a.AllConditions().Intersect(Values.OnDeathConditions).Any()))
            {
                if (await action.CheckConditions(state, kill))
                {
                    var actionNews = await action.DoOperations(state, kill);
                    var actionKills = actionNews.GetKills();
                    actionKills.ForEach(stack.Push);
                    dailyNews.Collect(actionNews);
                }
            }
        }

        state.LatestNews.Collect(dailyNews);

        if (city.GetRule(RuleName.KillOnDeathNoHeal).Accepted)
        {
            state.LatestNews.FactKills = state.LatestNews.FactKills.Concat(dailyNews.GetKills()).ToArray();
        }
        else
        {
            CalcNightKills();
        }
    }

    private async Task<bool> IsGameEnd() => GetWinnerGroup() != null || await host.IsGameEnd(state);

    private (bool success, Group winner) CheckWinRule(RuleName winRuleName)
    {
        var winRule = city.GetRule(winRuleName);
        if (winRule.Accepted)
        {
            var win = state.Players.Where(p => p.TopGroup.Name == winRule.Values[0]).Count();
            var notWin = state.Players.Where(p => p.TopGroup.Name != winRule.Values[0]).Count();
            var civilian = state.Players.Where(p => winRule.GetListValues(1).Contains(p.Role.Name)).Count();

            if (notWin == civilian && win >= civilian)
                return (true, city.GetTopGroup(winRule.Values[0]));
        }

        return (false, null!);
    }

    /// <summary>
    /// todo: config
    /// </summary>
    private Group? GetWinnerGroup()
    {
        var counts = state.Players.GroupBy(p => p.TopGroup).ToDictionary(gv => gv.Key, gv => gv.Count());

        if (counts.Keys.Count == 1)
            return counts.Keys.First();

        var (win, group) = CheckWinRule(RuleName.MafiaWin);
        if (win) return group;

        (win, group) = CheckWinRule(RuleName.ManiacWin);
        if (win) return group;

        return null;
    }

    public async Task Start()
    {
        var players0 = host.GetGameRoles().Select((role, i) => city.CreatePlayer(city.GetRole(role), $"P{(i + 1).ToString().PadLeft(2, '0')}")).ToArray();

        state = new State 
        { 
            Host = host, 
            City = city,
            Players0 = players0, 
            Players = players0.ToList(), 
            IsDay = true, 
            DayNumber = 1, 
            News = new(),
            IsActive = true
        };

        await host.StartGame(state);

        while (!state.Stopping)
        {
            state.IsDay = true;
            if (state.DayNumber > 1)
            {
                CalcNightKills();
                await PlayOnDeathKills();
                ApplyKills();
            }
            state.IsMorning = true;
            await host.NotifyDayStart(state);
            await host.NotifyCityAfterNight(state);
            if (await IsGameEnd())
            {
                await host.NotifyGameEnd(state, GetWinnerGroup()!);
                break;
            }
            await PlayDay();
            CalcDayKills();
            await PlayOnDeathKills();
            ApplyKills();
            state.IsEvening = true;
            await host.NotifyCityAfterDay(state);
            if (await IsGameEnd())
            {
                await host.NotifyGameEnd(state, GetWinnerGroup()!);
                break;
            }
            state.IsNight = true;
            await host.NotifyNightStart(state);
            await PlayNight();

            state.DayNumber++;
        }

        state.IsActive = false;
        
        if (stopping != null)
            stopping.SetResult();
    }

    private TaskCompletionSource? stopping = null;
    public async Task Stop()
    {
        if (!IsActive)
            return;

        stopping = new TaskCompletionSource();
        state.Stopping = true;
        await stopping.Task;
        stopping = null;
    }
}
