using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Host.Extensions;
using Host.Model;
using Host.Views;
using Mafia;
using Mafia.Extensions;
using Mafia.Libraries;
using Mafia.Model;
using Microsoft.Extensions.Options;

namespace Host.ViewModel;

public partial class HostViewModel : NotifyPropertyChanged
{
    private const string ReplaySecureKey = "Mafia_Host_Replays";

    private Random rnd;
    private readonly City city;
    private HostOptions options;
    private string navigationPath;
    private List<Replay> replays;
    
    public Dictionary<KnownRoleKey, string> KnownRoles { get; }
    public Dictionary<string, string> Messages { get; }

    public HostViewModel(Game game, City city, IOptions<HostOptions> options)
    {
        rnd = new Random();
        this.game = game;
        this.city = city;
        this.options = options.Value;
        HintColor = this.options.CityColor;
        SelectedPlayerRoleMessageColor = this.options.CityColor;
        Messages = this.options.Messages.ToDictionary(v => v.Name, v => v.Text);
        KnownRoles = this.options.KnownRoles.ToDictionary(v => v.Key, v => v.Name);

        Task.Run(Init).Wait();
    }

    public ICommand NavigatedCommand => new Command(Shell_Navigated);

    private async Task Init()
    {
        replays = await ReadReplays();
        users = await ReadUsers();

        if (users.Count < options.PresetPlayerCount)
        {
            var lastPlay = DateTime.Now.Date;
            var newUsers = Enumerable.Range(users.Count + 1, options.PresetPlayerCount - users.Count + 1).Select(i => new User { Nick = $"Nick{i}", LastPlay = lastPlay }).ToArray();
            users.AddRange(newUsers);
            await WriteUsers(users);
        }

        ActiveUsers = users.OrderBy(u => u.Nick).Take(options.PresetPlayerCount).Select(GetActiveUser).ToList();
    }

    private void Shell_Navigated(object path)
    {
        navigationPath = path.ToString()!;

        switch (navigationPath)
        {
            case "//pages/UserView":
                OnTabUsersNavigated();
                break;

            case "//pages/StartGameView":
                OnTabStartGameNavigated();
                break;

            case "//pages/GameView":
                OnTabGameNavigated();
                break;
        }
    }

    private void RefreshCommands() => GetType().GetProperties().Where(p => p.PropertyType == typeof(ICommand)).ForEach(p => Changed(p.Name));

    private Color GetOperationColor(string? operation) =>
        options.OperationColors.FirstOrDefault(v => v.Operation == operation)?.Color ??
        options.OperationColors.Single(v => v.Operation == "Unknown").Color;

    private void Log(string text)
    {
        Debug.WriteLine(text);
    }

    private void SaveGameReplay(State state)
    {
        var players = state.Players0.ToList();
        int[] GetWhom(Select s) => s.Whom.Select(p => players.IndexOf(p)).ToArray();
        int GetWho(Select s) => s.IsCity ? -1 : players.IndexOf(s.Who);
        var items = state.News.SelectMany(n => n.Selects.Select(s => $"({GetWho(s)}, [{GetWhom(s).SJoin(", ")}])")).SJoin(", ");
        Debug.WriteLine($"[{items}]");
    }


    private async Task<List<Replay>> ReadReplays()
    {
        var json = await SecureStorage.Default.GetAsync(ReplaySecureKey);

        if (!json.HasText())
            return [];

        return json.FromJson<List<Replay>>();
    }

    private async Task WriteReplace(ICollection<Replay> replays)
    {
        await SecureStorage.Default.SetAsync(ReplaySecureKey, replays.ToJson());
    }
}
