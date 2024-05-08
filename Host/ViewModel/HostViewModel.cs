using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Host.Model;
using Mafia;
using Mafia.Extensions;
using Mafia.Libraries;
using Mafia.Model;
using Microsoft.Extensions.Options;

namespace Host.ViewModel;

/// <summary>
/// todo: view code for host
/// </summary>
public class HostViewModel : NotifyPropertyChanged, IHost
{
    private Random rnd;
    private readonly Game game;
    private readonly City city;
    private HostOptions options;


    private string _hostHint;
    public string HostHint { get => _hostHint; set { _hostHint = value; Changed(nameof(HostHint)); } }

    private string _text;
    public string Text { get => _text; set { _text = value; Changed(); } }

    private string _playerInfo;
    public string PlayerInfo { get => _playerInfo; set { _playerInfo = value; Changed(); } }

    public Player0[] Roles { get; private set; }
    public ActivePlayer[] ActivePlayers { get; private set; }

    private (string name, int count)[] GetRolesPreset(int n)
    {
        return RoleValues.GetRolesPreset(["DonMafia", "BumMafia", "Maniac", "Commissar", "Doctor"], "Mafia", "Civilian", n, 3.5);
    }

    public HostViewModel(Game game, City city, IOptions<HostOptions> options)
    {
        rnd = new Random();
        this.game = game;
        this.city = city;
        this.options = options.Value;

        InitRoles();
    }


    private void InitRoles()
    {
        Roles = city.AllRoles()
            .Select(r => (role: r, preset: GetRolesPreset(15).FirstOrDefault(rr => rr.name == r.Name)))
            .Select(v => new Player0(v.role, f => RefreshPlayersInfo()) { IsSelected = v.preset.count > 0, Count = v.preset.count > 0 ? v.preset.count : 1 }).ToArray();

        RefreshPlayersInfo();
    }

    TaskCompletionSource? interactor = null;
    private void ContinueGame() => interactor?.SetResult();

    public ICommand Continue => new Command(async () =>
    {
        await Task.Delay(20);
        ContinueGame();
    }, () => game.IsActive);

    public async Task Interact(string message)
    {
        HostHint = message;

        while (interactor != null)
        {
            Debug.WriteLine("warn: async error");
            await Task.Delay(10);
        }
        
        interactor = new TaskCompletionSource();
        
        if (!game.Stopping)
            await interactor.Task;

        interactor = null;
    }

    void RefreshPlayersInfo()
    {
        if (Roles == null)
            return;

        var count = Roles.Where(r => r.IsSelected).Sum(r => r.Count);

        PlayerInfo = $"Players: {count}";
    }

    private bool activePlayersSilent = false;
    void RefreshActivePlayerInfo(string name)
    {
        if (!activePlayersSilent)
        {
            activePlayersSilent = true;

            if (ActivePlayers.Where(p => p.IsSelected).Any())
            {
                ActivePlayers.Where(p => !p.IsSelected).ForEach(p => p.IsEnabled = false);
            }
            else
            {
                ActivePlayers.ForEach(p => p.IsEnabled = true);
            }

            activePlayersSilent = false;
        }
    }

    public ICommand StartNewGame => new Command(async () =>
    {
        var stopTask = game.Stop();
        ContinueGame();
        await stopTask;

        var seed = new Random().Next();
        Text = "";
        WriteLine($"\r\n'{city.Name}' game {seed}");
        ChangeSeed(seed);

        game.Start().ContinueWith(_ => RefreshCommands()).NoWait();

        await Task.Delay(50);
        RefreshCommands();
        RefreshPlayersInfo();

        await Shell.Current.GoToAsync("//pages/GameView");
    });

    private void RefreshCommands() => GetType().GetProperties().Where(p => p.PropertyType == typeof(ICommand)).ForEach(p => Changed(p.Name));
    

    private void WriteLine(string text)
    {
        Text += $"{text}\r\n";
    }

    public void ChangeSeed(int seed)
    {
        rnd = new Random(seed);
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

        var gameRoles = Roles.Where(r => r.IsSelected).SelectMany(r => Enumerable.Range(0, r.Count).Select(_ => r.Role.Name)).ToArray();
        gameRoles.Shaffle(17, rnd);

        return gameRoles.Select((role, i) => (users[i], role)).ToArray();
    }

    public void StartGame(State state) 
    {
        ActivePlayers = state.Players.Select(p => new ActivePlayer(p, RefreshActivePlayerInfo)).OrderBy(p=>p.Player.Group.Name).ThenBy(p=>p.Player.Role.Rank).ToArray();
        Changed(nameof(ActivePlayers));
    }

    private async Task TellTheNews(State state)
    {
        if (!state.HasNews)
        {
            await Interact("Hello city");
            WriteLine($"Game players: {state.Players.SJoin(", ")}");
        }
        else
        {
            await Interact($"Where killed: {state.LatestNews.Killed.SJoin(", ")}");
            WriteLine($"Where killed: {state.LatestNews.Killed.SJoin(", ")}");
            WriteLine($"Alive players: {state.Players.SJoin(", ")}");
        }
    }

    public async Task NotifyCityAfterNight(State state)
    {
        await TellTheNews(state);
    }

    public async Task NotifyCityAfterDay(State state)
    {
        await TellTheNews(state);
    }

    public async Task NotifyDayStart(State state)
    {
        if (state.DayNumber > 1)
        {
            await AskCityToWakeUp();

            WriteLine($"===== </night {state.DayNumber}> =====");
        }

        WriteLine($"===== <day {state.DayNumber}> =====");        
    }

    public async Task NotifyNightStart(State state)
    {
        await AskCityToFallAsleep();

        WriteLine($"===== </day {state.DayNumber}> =====");
        WriteLine($"===== <night {state.DayNumber}> =====");
    }

    public async Task<bool> IsGameEnd(State state)
    {
        // Ведущий может остановить игру в результате математической победы (2 мафии, 2 мирных)
        return false;
    }

    public async Task NotifyGameEnd(State state, Group winnerGroup)
    {
        WriteLine($"GameEnd, the winner is {winnerGroup.Name}");
        WriteLine($"===== </day {state.DayNumber}> =====");
    }

    public async Task<bool> AskCityToSkip(State state)
    {
        var skip = rnd.NextDouble() < 0.1;

        if (skip && options.CitySelections)
            WriteLine($"City select nobody");

        return skip;
    }

    public async Task<Player> AskCityToSelect(State state)
    {
        ActivePlayers.ForEach(p =>
        {
            p.IsEnabled = true;
            p.IsSelected = false;
        });

        await Interact($"City select somebody to kill");

        if (options.HostInstructions)
            WriteLine($"City select somebody to kill");

        var selected = state.Players[rnd.Next(state.Players.Count)];

        if (options.CitySelections)
            WriteLine($"City --> {selected}");

        return selected;
    }

    public async Task<Player[]> GetNeighbors(State state, Player player)
    {
        var selected = state.GetNeighborPlayers(player);

        if (options.CitySelections)
            WriteLine($"{player} --> {selected.SJoin(", ")}");

        return selected;
    }

    public async Task<bool> AskToSkip(State state, Player player)
    {
        await Interact($"{player}, do you want to select somebody?");

        var skip = rnd.NextDouble() < 0.1;

        if (skip && options.CitySelections)
            WriteLine($"{player} select nobody");

        return skip;
    }

    public async Task<Player[]> AskToSelect(State state, Player player)
    {
        await AskToWakeUp(state, player);

        await Interact($"Whom {player} would like to select?");

        if (options.HostInstructions)
            WriteLine($"Whom {player} would like to select?");

        await AskToFallAsleep(state, player);

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
            WriteLine($"{player} --> {(selected is [] ? "nobody" : selected.SJoin(", "))}");

        return selected;
    }

    private async Task AskCityToWakeUp()
    {
        ActivePlayers.ForEach(p => 
        { 
            p.IsEnabled = false; 
            p.IsSelected = false; 
        });

        await Interact($"City, wake up please");

        if (options.HostInstructions)
            WriteLine($"City, wake up please");
    }

    private async Task AskCityToFallAsleep()
    {
        ActivePlayers.ForEach(p =>
        {
            p.IsEnabled = false;
            p.IsSelected = false;
        });

        await Interact($"City, fall asleep please");

        if (options.HostInstructions)
            WriteLine($"City, fall asleep please");
    }

    private async Task AskToWakeUp(State state, Player player)
    {
        await Interact($"{player}, wake up please");

        if (state.IsNight && options.HostInstructions)
            WriteLine($"{player}, wake up please");
    }

    private async Task AskToFallAsleep(State state, Player player)
    {
        await Interact($"{player}, fall asleep please");

        if (state.IsNight && options.HostInstructions)
            WriteLine($"{player}, fall asleep please");
    }
}
