using System.Data;
using System.Diagnostics;
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

    public void ChangeSeed(int seed)
    {
        rnd = new Random(seed);
    }

    private string[] GetGameRoles((string name, int count)[] preset, int n, int k)
    {
        string[] roles = preset.Select(r => r.name).ToArray();
        string[] multipleRoles = ["Mafia", "Civilian"];

        var nn = n - roles.Length;
        var nMafia = nn / k;
        var nCivilian = nn - nMafia;

        var mafias = Enumerable.Range(0, nMafia).Select(_ => multipleRoles[0]);
        var civilians = Enumerable.Range(0, nCivilian).Select(_ => multipleRoles[1]);

        return roles.Concat(mafias).Concat(civilians).ToArray();
    }

    public (User, string)[] GetUserRoles()
    {
        var nMax = 20; // пользователи в базе данных
        var usersDataBase = Enumerable.Range(1, nMax + 1).Select(i => new User { Nick = $"Nick{i}" }).ToArray();
        var userList = usersDataBase.ToList();

        var n = 15; // пришло поиграть
        var users = Enumerable.Range(1, n + 1).Select(_ =>
        {
            var i = rnd.Next(userList.Count);
            var player = userList[i];
            userList.RemoveAt(i);
            return player;
        }).ToArray();
        
        (string name, int count)[] rolesPreset = [("DonMafia", 1), ("BumMafia", 1), ("Mafia", 1), ("Maniac", 1), ("Commissar", 1), ("Doctor", 1), ("Civilian", 4)];
        
        var gameRoles = GetGameRoles(rolesPreset, n, 4);
        gameRoles.Shaffle(17, rnd);

        return gameRoles.Select((role, i) => (users[i], role)).ToArray();
    }

    private void TellTheNews(State state)
    {
        if (!state.HasNews)
        {
            Debug.WriteLine($"Game players: {state.Players.SJoin(", ")}");
        }
        else
        {
            Debug.WriteLine($"Where killed: {state.LatestNews.Killed.SJoin(", ")}");
            Debug.WriteLine($"Alive players: {state.Players.SJoin(", ")}");
        }
    }

    public async Task NotifyCityAfterNight(State state)
    {
        AskCityToWakeUp();
        TellTheNews(state);

        //check game end
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
        Debug.WriteLine($"GameEnd, the winner is {winnerGroup.Name}");
        Debug.WriteLine($"===== </day {state.DayNumber}> =====");
    }

    public async Task<bool> AskCityToSkip(State state)
    {
        var skip = rnd.NextDouble() < 0.1;

        if (skip && options.CitySelections)
            Debug.WriteLine($"City select nobody");

        return skip;
    }

    public async Task<Player> AskCityToSelect(State state)
    {
        if (options.HostInstructions)
            Debug.WriteLine($"City select somebody to kill");

        var selected = state.Players[rnd.Next(state.Players.Count)];

        if (options.CitySelections)
            Debug.WriteLine($"City --> {selected}");

        return selected;
    }

    public async Task<Player[]> GetNeighbors(State state, Player player)
    {
        var selected = state.GetNeighborPlayers(player);

        if (options.CitySelections)
            Debug.WriteLine($"{player} --> {selected.SJoin(", ")}");

        return selected;
    }

    public async Task<bool> AskToSkip(State state, Player player)
    {
        var skip = rnd.NextDouble() < 0.1;

        if (skip && options.CitySelections)
            Debug.WriteLine($"{player} select nobody");

        return skip;
    }

    public async Task<Player[]> AskToSelect(State state, Player player)
    {
        AskToWakeUp(state, player);
        if (options.HostInstructions)
            Debug.WriteLine($"Whom {player} would like to select?");
        AskToFallAsleep(state, player);

        Player[] selected;
        var except = state.GetExceptPlayers(player);

        if (player.Is("Doctor") && !except.Contains(player))
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

        return selected;
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
