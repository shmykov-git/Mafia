using System.Data;
using System.Diagnostics;
using System.Windows.Input;
using Host.Model;
using Mafia;
using Mafia.Extensions;
using Mafia.Model;
using Microsoft.Extensions.Options;

namespace Host.ViewModel;

public partial class HostViewModel : NotifyPropertyChanged
{
    private const string ReplaySecureKey = "Mafia_Host_Replays";

    private Random rnd;
    private City city;
    private HostOptions options;
    private LanguageOption language;
    private string navigationPath;
    private List<Replay> replays;
    
    public Dictionary<KnownRoleKey, string> KnownRoles { get; }
    public Dictionary<string, string> Messages { get; private set; }

    public HostViewModel(Game game, IOptions<HostOptions> options)
    {
        rnd = new Random();
        this.game = game;
        this.options = options.Value;
        HintColor = this.options.Theme.CityColor;
        SelectedPlayerRoleMessageColor = this.options.Theme.CityColor;
        language = this.options.Languages.Single(l => l.Name == this.options.DefaultLanguage);
        Messages = language.Messages.ToDictionary(v => v.Name, v => v.Text);
        KnownRoles = language.KnownRoles.ToDictionary(v => v.Key, v => v.Name);

        Task.Run(Init).Wait();
    }

    public ICommand NavigatedCommand => new Command(Shell_Navigated);

    private async Task Init()
    {
        await LoadCityMaps();
        InitSettings();

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
        navigationPath = path.ToString();
        Debug.WriteLine($"Navigated: {navigationPath}");
    }

    private void RefreshCommands() => GetType().GetProperties().Where(p => p.PropertyType == typeof(ICommand)).ForEach(p => Changed(p.Name));

    private Color GetOperationColor(string? operation) =>
        options.Theme.OperationColors.FirstOrDefault(v => v.Operation == operation)?.Color ??
        options.Theme.OperationColors.Single(v => v.Operation == "Unknown").Color;

    private void Log(string text)
    {
        Debug.WriteLine(text);
    }

    private async Task SaveGameReplay(State state)
    {
        replays.Add(state.Replay);
        await WriteReplace(replays);
        Debug.WriteLine($"[{state.Replay}]");
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
