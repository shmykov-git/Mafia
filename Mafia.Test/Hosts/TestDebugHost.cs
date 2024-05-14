using System.Data;
using System.Diagnostics;
using Mafia.Extensions;
using Mafia.Libraries;
using Mafia.Model;
using Mafia.Test.Model;
using Microsoft.Extensions.Options;
using Action = Mafia.Model.Action;

namespace Mafia.Test.Hosts;

public class TestDebugHost : IHost
{
    private Random rnd;
    private readonly City city;
    private TestDebugOptions options;

    public TestDebugHost(City city, IOptions<TestDebugOptions> options)
    {
        rnd = new Random(options.Value.Seed);
        this.city = city;
        this.options = options.Value;
    }

    public void ChangeSeed(int seed)
    {
        rnd = new Random(seed);
    }

    public async Task StartGame(State state)
    {
        var n = state.Players0.Length;
        var users = Enumerable.Range(0, n).Select(i => new User { Nick = $"U{(i).ToString().PadLeft(2, '0')}", LastPlay = DateTime.Now }).ToArray();
        state.Players0.ForEach((p, i) => p.User = users[i]);
    }

    public string[] GetGameRoles()
    {
        return options.RolesPreset.SelectMany(v => Enumerable.Range(0, v.count).Select(_ => v.name)).ToArray();
    }

    private void TellTheNews(State state)
    {
        if (!state.HasNews)
        {
            Debug.WriteLine($"Game players: {state.Players.SJoin(", ")}");
        }
        else
        {
            Debug.WriteLine($"Where killed: {state.LatestNews.FactKills.SJoin(", ")}");
            Debug.WriteLine($"Alive players: {state.Players.SJoin(", ")}");
        }
    }

    public async Task NotifyCityAfterNight(State state)
    {
        AskCityToWakeUp();
        TellTheNews(state);
    }

    public async Task NotifyCityAfterDay(State state)
    {
        //check game end
        TellTheNews(state);

        AskCityToFallAsleep();
    }

    public async Task NotifyDayStart(State state)
    {
        if (state.DayNumber > 1)
            Debug.WriteLine($"===== </night {state.DayNumber}> =====");

        Debug.WriteLine($"===== <day {state.DayNumber}> =====");
    }

    public async Task NotifyNightStart(State state)
    {
        Debug.WriteLine($"===== </day {state.DayNumber}> =====");
        Debug.WriteLine($"===== <night {state.DayNumber}> =====");
    }

    public async Task<bool> IsGameEnd(State state)
    {
        // Ведущий может остановить игру в результате математической победы (2 мафии, 2 мирных)
        return false;
    }

    public async Task NotifyGameEnd(State state, Group winnerGroup)
    {
        var players = state.Players0.ToList();
        int[] GetWhom(Select s) => s.Whom.Select(p=>players.IndexOf(p)).ToArray();
        int GetWho(Select s) => s.IsCity ? -1 : players.IndexOf(s.Who);

        var items = state.News.SelectMany(n=>n.Selects.Select(s=> $"({GetWho(s)}, [{GetWhom(s).SJoin(", ")}])")).SJoin(", ");

        Debug.WriteLine($"GameEnd, the winner is {winnerGroup.Name}");
        Debug.WriteLine($"[{items}]");
        Debug.WriteLine($"===== </day {state.DayNumber}> =====");
    }

    public async Task<User[]> AskCityToSelect(State state, CityAction action, string operation)
    {
        if (options.HostInstructions)
            Debug.WriteLine($"City select somebody to kill{(action.IsSkippable() ? " or skip" : "")}");

        var skip = action.IsSkippable() && rnd.NextDouble() < 0.1;

        Player[] selected = skip
            ? []
            : [state.Players[rnd.Next(state.Players.Count)]];

        if (options.CitySelections)
            Debug.WriteLine($"City --> {(selected is [] ? "nobody" : selected.SJoin(", "))}");

        return selected.Select(p => p.User).ToArray();
    }

    public async Task<User[]> GetNeighbors(State state, Player player, Action action, string operation)
    {
        var selected = state.GetNeighborPlayers(player);

        if (options.CitySelections)
            Debug.WriteLine($"{player} --> {selected.SJoin(", ")}");

        return selected.Select(p => p.User).ToArray();
    }

    public async Task<User[]> AskToSelect(State state, Player player, Action action, string operation)
    {
        AskToWakeUp(state, player);

        if (options.HostInstructions)
            Debug.WriteLine($"Whom {player} would like to select{(action.IsSkippable() ? " or skip" : "")}?");

        Player[] selected;

        var skip = action.IsSkippable() && rnd.NextDouble() < 0.1;
        var except = state.GetExceptPlayers(player);

        if (skip)
        {
            selected = [];
        }
        else if (player.Is("Doctor") && !except.Contains(player))
        {
            selected = [player];
        }
        else
        {
            var otherTeams = state.GetOtherTeams(player).Except(except).ToArray();

            selected = otherTeams.Length > 0
                ? [otherTeams[rnd.Next(otherTeams.Length)]]
                : [];
        }

        if (options.CitySelections)
            Debug.WriteLine($"{player} --> {(selected is [] ? "nobody" : selected.SJoin(", "))}");

        AskToFallAsleep(state, player);

        return selected.Select(p => p.User).ToArray();
    }

    private void AskCityToWakeUp()
    {
        if (options.HostInstructions)
            Debug.WriteLine($"City, wake up please");
    }

    private void AskCityToFallAsleep()
    {
        if (options.HostInstructions)
            Debug.WriteLine($"City, fall asleep please");
    }

    private void AskToWakeUp(State state, Player player)
    {
        if (state.IsNight && options.HostInstructions)
            Debug.WriteLine($"{player}, wake up please");
    }

    private void AskToFallAsleep(State state, Player player)
    {
        if (state.IsNight && options.HostInstructions)
            Debug.WriteLine($"{player}, fall asleep please");
    }

}
