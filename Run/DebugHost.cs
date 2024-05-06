using System.Diagnostics;
using System.Numerics;
using System.Xml.Linq;
using Mafia.Extensions;
using Mafia.Model;
using Microsoft.Extensions.Options;

namespace Run;

/// <summary>
/// todo: view code for host
/// </summary>
public class DebugHost : IHost
{
    private Random rnd;
    private readonly City city;
    private RunOptions options;

    public DebugHost(City city, IOptions<RunOptions> options)
    {
        rnd = new Random(options.Value.Seed);
        this.city = city;
        this.options = options.Value;
    }

    public Player[] GetPlayers()
    {
        var nMax = 20;
        var users = Enumerable.Range(1, nMax + 1).Select(i => new User { Nick = $"Nick{i}" }).ToArray();

        var listP = users.ToList();

        var n = 15;
        var gamePlayers = Enumerable.Range(1, n + 1).Select(_ =>
        {
            var i = rnd.Next(listP.Count);
            var player = listP[i];
            listP.RemoveAt(i);
            return player;
        }).ToArray();

        var civilianRole = city.GetRole("Civilian");
        var mafiaRole = city.GetRole("Mafia");

        var nn = n - city.AllRoles().Count();
        var nMafia = nn / 3;
        var nCivilian = nn - nMafia;

        var mafias = Enumerable.Range(0, nMafia).Select(_ => mafiaRole);
        var civilians = Enumerable.Range(0, nCivilian).Select(_ => civilianRole);

        var gameRoles = city.AllRoles().Concat(mafias).Concat(civilians).ToArray();

        gameRoles.Shaffle(17, rnd);

        var players = gameRoles.Select((r, i) => new Player { User = gamePlayers[i], Role = gameRoles[i], Group = city.GetGroup(gameRoles[i]) }).ToArray();

        return players;
    }

    public Player AskToSelect(State state, Player player)
    {
        AskToWakeUp(state, player);
        if (options.HostInstructions)
            Debug.WriteLine($"Whom {player} would like to select?");
        AskToFallAsleep(state, player);

        Player selected;

        if (player.Is("Doctor"))
        {
            selected = player;
        }
        else
        {
            var otherTeams = state.GetOtherTeams(player);
            selected = otherTeams[rnd.Next(otherTeams.Length)];
        }

        if (options.CitySelections)
            Debug.WriteLine($"{player} --> {selected}");

        return selected;
    }

    public void NotifyCityAfterNight(State state)
    {
        if (state.DayNumber > 1)
            Debug.WriteLine($"===== </night {state.DayNumber}> =====");

        Debug.WriteLine($"===== <day {state.DayNumber}> =====");
        AskCityToWakeUp();

        //check game end
    }

    public void NotifyCityAfterDay(State state)
    {
        //check game end

        AskCityToFallAsleep();
        Debug.WriteLine($"===== </day {state.DayNumber}> =====");
        Debug.WriteLine($"===== <night {state.DayNumber}> =====");
    }

    public bool AskCityToSkip(State state)
    {
        var skip = rnd.NextDouble() < 0.1;

        if (skip && options.CitySelections)
            Debug.WriteLine($"City skip kill");

        return skip;
    }

    public Player AskCityToSelect(State state)
    {
        if (options.HostInstructions)
            Debug.WriteLine($"City select somebody to kill");

        var selected = state.Players[rnd.Next(state.Players.Count)];

        if (options.CitySelections)
            Debug.WriteLine($"City --> {selected}");

        return selected;
    }

    public Player AskToSelectNotSelf(State state, Player player)
    {
        AskToWakeUp(state, player);
        if (options.HostInstructions)
            Debug.WriteLine($"Whom {player} would like to select except his self?");
        AskToFallAsleep(state, player);

        var otherTeams = state.GetOtherTeams(player);

        var selected = otherTeams[rnd.Next(otherTeams.Length)];

        if (options.CitySelections)
            Debug.WriteLine($"{player} --> {selected}");

        return selected;
    }

    public Player[] GetNeighbors(State state, Player player)
    {
        var selected = state.GetNeighborPlayers(player);

        if (options.CitySelections)
            Debug.WriteLine($"{player} --> {selected.SJoin(", ")}");

        return selected;
    }

    public bool AskToSkip(State state, Player player)
    {
        var skip = rnd.NextDouble() < 0.1;

        if (skip && options.CitySelections)
            Debug.WriteLine($"{player} select nobody");

        return skip;
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
