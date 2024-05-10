using System.Data;
using System.Diagnostics;
using Mafia.Extensions;
using Mafia.Libraries;
using Mafia.Model;
using Microsoft.Extensions.Options;
using Action = Mafia.Model.Action;

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
    
    public async Task StartGame(State state) { }
    
    private (string name, int count)[] GetRolesPreset(int n)
    {
        return RoleValues.GetRolesPreset(["Дон", "Бомж", "Маньяк", "Комиссар", "Доктор"], "Мафия", "Мирный", n, 3.5);
        //return RoleValues.GetRolesPreset(["DonMafia", "BumMafia", "Maniac", "Commissar", "Doctor"], "Mafia", "Civilian", n, 3.5);
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

        (string name, int count)[] rolesPreset = GetRolesPreset(n);

        var gameRoles = rolesPreset.SelectMany(v => Enumerable.Range(0, v.count).Select(_ => v.name)).ToArray();
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

    public async Task<Player[]> AskCityToSelect(State state, CityAction action)
    {
        if (options.HostInstructions)
            Debug.WriteLine($"City select somebody to kill{(action.IsSkippable() ? " or skip" : "")}");

        var skip = action.IsSkippable() && rnd.NextDouble() < 0.1;
        
        Player[] selected = skip 
            ? []
            : [state.Players[rnd.Next(state.Players.Count)]];

        if (options.CitySelections)
            Debug.WriteLine($"City --> {(selected is [] ? "nobody" : selected.SJoin(", "))}");

        return selected;
    }

    public async Task<Player[]> GetNeighbors(State state, Player player, Action action)
    {
        var selected = state.GetNeighborPlayers(player);

        if (options.CitySelections)
            Debug.WriteLine($"{player} --> {selected.SJoin(", ")}");

        return selected;
    }

    public async Task<Player[]> AskToSelect(State state, Player player, Action action)
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
