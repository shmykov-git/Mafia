using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Numerics;
using System.Windows.Input;
using Host.Model;
using Mafia;
using Mafia.Extensions;
using Mafia.Model;
using Microsoft.Extensions.Options;

namespace Host.ViewModel;

/// <summary>
/// todo: view code for host
/// </summary>
public class HostViewModel : IHost, INotifyPropertyChanged
{
    private Random rnd;
    private readonly Game game;
    private readonly City city;
    private HostOptions options;


    private string _hostHint;
    public string HostHint { get => _hostHint; set { _hostHint = value; Changed(nameof(HostHint)); } }

    private string _text;
    public string Text { get => _text; set { _text = value; Changed(nameof(Text)); } }

    private string _playerInfo;
    public string PlayerInfo { get => _playerInfo; set { _playerInfo = value; Changed(nameof(PlayerInfo)); } }

    public SelectedRole[] Roles { get; }

    private (string name, int count)[] rolesPreset = [("DonMafia", 1), ("BumMafia", 1), ("Mafia", 1), ("Maniac", 1), ("Commissar", 1), ("Doctor", 1), ("Civilian", 4)];

    public HostViewModel(Game game, City city, IOptions<HostOptions> options)
    {
        rnd = new Random();
        this.game = game;
        this.city = city;
        this.options = options.Value;

        Roles = city.AllRoles()
            .Select(r => (role: r, preset: rolesPreset.FirstOrDefault(rr => rr.name == r.Name)))
            .Select(v => new SelectedRole(v.role, f => RefreshPlayerInfo()) { IsSelected = v.preset.count > 0, Count = v.preset.count > 0 ? v.preset.count : 1 }).ToArray();

        RefreshPlayerInfo();
    }



    public ICommand Continue => new Command(() =>
    {
        awaiter?.SetResult();
    });

    TaskCompletionSource? awaiter = null;
    public async Task Interact(string message)
    {
        HostHint = message;

        awaiter = new TaskCompletionSource();
        await awaiter.Task;
        awaiter = null;
    }


    void RefreshPlayerInfo()
    {
        if (Roles == null)
            return;

        var count = Roles.Where(r => r.IsSelected).Sum(r => r.Count);

        PlayerInfo = $"Players: {count}";
    }

    public ICommand StartNewGame => new Command(async () =>
    {
        await Shell.Current.GoToAsync("//pages/GameView");

        var seed = new Random().Next();

        Text = "";
        WriteLine($"\r\n'{city.Name}' game {seed}");
        ChangeSeed(seed);
        game.Start();
    });

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
            HostHint = "WakeUp City";
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
        await Interact($"City, wake up please");

        if (options.HostInstructions)
            WriteLine($"City, wake up please");
    }

    private async Task AskCityToFallAsleep()
    {
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


    public event PropertyChangedEventHandler? PropertyChanged;
    public void Changed(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

}
