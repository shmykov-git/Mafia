using System.Data;
using System.Diagnostics;
using FluentAssertions;
using Mafia.Extensions;
using Mafia.Model;
using Mafia.Test.Model;
using Microsoft.Extensions.Options;
using Action = Mafia.Model.Action;

namespace Mafia.Test.Hosts;

public class TestHost : IHost
{
    private Random rnd;
    private readonly City city;
    private readonly TestOptions options;
    private int eventIndex = 0;

    public TestHost(City city, IOptions<TestOptions> options)
    {
        this.city = city;
        this.options = options.Value;
    }

    public void ChangeSeed(int seed)
    {
    }

    public async Task StartGame(State state)
    {
        var n = state.Players0.Length;
        var users = Enumerable.Range(0, n).Select(i => new User { Nick = $"U{(i).ToString().PadLeft(2, '0')}", LastPlay = DateTime.Now }).ToArray();
        state.Players0.ForEach((p, i) => p.User = users[i]);
    }

    public string[] GetGameRoles()
    {
        return options.TestRoles.SelectMany(v => Enumerable.Range(0, v.count).Select(_ => v.name)).ToArray();
    }

    public async Task NotifyCityAfterNight(State state)
    {
    }

    public async Task NotifyCityAfterDay(State state)
    {
    }

    public async Task NotifyDayStart(State state)
    {
    }

    public async Task NotifyNightStart(State state)
    {
    }

    public async Task<bool> IsGameEnd(State state)
    {
        return false;
    }

    public async Task NotifyGameEnd(State state, Group winnerGroup)
    {
        //state.City.Groups.ToList().IndexOf(winnerGroup).Should().Be(options.WinGroup);
        eventIndex.Should().Be(options.Selections.Length);
    }
    public void RolledBack(State state) { }
    private Player[] GetSelectedPlayers(State state) => options.Selections[eventIndex].Item2.Select(i => state.Players0[i]).ToArray();

    private User[] GetSelectedUsers(State state, Player? who)
    {
        if (who != null)
        {
            state.Players0.ToList().IndexOf(who).Should().Be(options.Selections[eventIndex].Item1);
        }
        else
        {
            (-1).Should().Be(options.Selections[eventIndex].Item1);
        }

        var players = GetSelectedPlayers(state);
        players.ForEach(p => state.Players.Should().Contain(p));

        if (options.Debug)
            Debug.WriteLine($"{(who == null ? "city" : who.ToString())} --> {players.SJoin(", ")}");

        eventIndex++;
        return players.Select(p => p.User).ToArray();
    }

    public async Task<User[]> AskCityToSelect(State state, CityAction action, string operation)
    {
        return GetSelectedUsers(state, null);
    }

    public async Task<User[]> GetNeighbors(State state, Player player, Action action, string operation)
    {
        return GetSelectedUsers(state, player);
    }

    public async Task<User[]> AskToSelect(State state, Player player, Action action, string operation)
    {
        return GetSelectedUsers(state, player);
    }
}
