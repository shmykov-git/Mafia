using System.Diagnostics;
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
        state.News.Add(news); // new latest news

        foreach (var action in city.DayActions)
        {
            if (await action.CheckConditions(state))
                news.Collect(await action.DoOperations(state));
        }

        CalcKills();
    }

    private async Task PlayNight()
    {
        var news = new DailyNews();
        state.News.Add(news); // new latest news

        CalcsBeforeNight();

        foreach (var group in city.NightEvents.Select(city.GetGroup))
        {
            foreach (var player in state.GetGroupActivePlayers(group))
            {
                foreach (var action in player.Role.AllActions())
                {
                    if (await action.CheckConditions(state, player))
                        news.Collect(await action.DoOperations(state, player));
                    else
                        news.Collect(await action.GetFailedActionNews(state, player));
                }
            }
        }

        CalcKills();
    }

    private async Task PlayOnDeathKills()
    {
        HashSet<Player> set = new();
        var stack = new Stack<Player>();
        state.LatestNews.FactKills.ForEach(stack.Push);
        state.LatestNews.FactKills.ForEach(k => set.Add(k));

        while (stack.TryPop(out var kill))
        {
            var dailyNews = new DailyNews();

            foreach (var action in kill.Role.AllActions().Where(a => a.AllConditions().Intersect(Values.OnDeathConditions).Any()))
            {
                if (await action.CheckConditions(state, kill))
                {
                    var actionNews = await action.DoOperations(state, kill);
                    var actionKills = actionNews.GetKills();
                    actionKills.Where(p => !set.Contains(p)).ForEach(stack.Push);
                    dailyNews.Collect(actionNews);
                }
            }

            state.LatestNews.Collect(dailyNews);
            CalcOnDeathKills(dailyNews);
        }
    }

    private void CalcKills()
    {
        state.DoKnowAllLatestWhom();
        state.LatestNews.FactKills = state.GetLatestFactKills();
    }

    private void CalcOnDeathKills(DailyNews onDeathDailyNews)
    {
        state.DoKnowAllLatestWhom();

        if (city.GetRule(RuleName.KillOnDeathNoHeal).Accepted)
        {
            state.LatestNews.FactKills = state.LatestNews.FactKills.Concat(onDeathDailyNews.GetKills()).ToArray();
        }
        else
        {
            state.LatestNews.FactKills = state.GetLatestFactKills();
        }
    }

    private void CalcsBeforeNight()
    {
        state.LatestNews.KillGroups = state.GetKillerGroups();
    }

    private void ApplyKills()
    {
        foreach(var player in state.LatestNews.FactKills)
            state.Players.Remove(player);
    }

    private async Task<bool> IsGameEnd() => state.Players.Count == 0 || GetWinnerGroup() != null || await host.IsGameEnd(state);

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
            Replay = new Replay() { MapName = city.Name, MapVersion = city.Version, StartTime = DateTime.Now, EndTime = DateTime.Now },
            City = city,
            Players0 = players0, 
            Players = players0.ToList(), 
            IsDay = true, 
            DayNumber = 1, 
            News = new(),
            IsActive = true
        };

        await host.StartGame(state);

        async Task<bool> Day()
        {
            state.IsDay = true;
            state.IsMorning = true;
            await host.NotifyDayStart(state);
            await host.NotifyCityAfterNight(state);

            if (await IsGameEnd())
            {
                await host.NotifyGameEnd(state, GetWinnerGroup()!);
                return true;
            }
            await PlayDay();            
            await PlayOnDeathKills();
            ApplyKills();
            state.IsEvening = true;
            await host.NotifyCityAfterDay(state);

            if (await IsGameEnd())
            {
                await host.NotifyGameEnd(state, GetWinnerGroup()!);
                return true;
            }

            await host.NotifyNightStart(state);

            return false;
        }

        async Task Night()
        {
            state.IsNight = true;
            await PlayNight();
            await PlayOnDeathKills();
            ApplyKills();
        }

        void Rollback()
        {
            state.Players.AddRange(state.LatestNews.FactKills);
            state.News.RemoveAt(state.News.Count - 1);
            host.RolledBack(state);
        }

        void UpdateStateReplay()
        {
            if (state.IsFirstDay)
                return;

            if (state.Replay.Players.Length == 0)
                state.Replay.Players = state.Players0.Select(p => (p.User!.Nick, p.Role.Name)).ToArray();

            var players = state.Players0.ToList();
            int[] GetWhom(Select s) => s.Whom.Select(p => players.IndexOf(p)).ToArray();
            int GetWho(Select s) => s.IsCity ? -1 : players.IndexOf(s.Who);

            state.Replay.Selections = state.News.Select(n => n.AllSelects().Select(s => (GetWho(s), GetWhom(s))).ToArray()).ToArray();
            state.Replay.EndTime = DateTime.Now;
        }

        while (!state.Stopping)
        {
            var needBreak = false;
            do
            {
                state.RollingBack = false;
                needBreak = await Day();
                
                if (state.RollingBack)
                    Rollback();
            } while (state.RollingBack);

            if (needBreak)
                break;

            do
            {
                state.RollingBack = false;
                await Night();

                if (state.RollingBack)
                    Rollback();
            } while (state.RollingBack);

            if (!state.IsFirstDay)
                UpdateStateReplay();

            state.DayNumber++;
        }

        UpdateStateReplay();
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
