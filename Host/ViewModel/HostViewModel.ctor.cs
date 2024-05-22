using System.Data;
using System.Diagnostics;
using System.Windows.Input;
using Host.Libraries;
using Host.Model;
using Host.Permission;
using Mafia;
using Mafia.Extensions;
using Mafia.Libraries;
using Mafia.Model;
using Mafia.Services;
using Microsoft.Extensions.Options;

namespace Host.ViewModel;

public partial class HostViewModel : NotifyPropertyChanged, ICity
{
    private const string UsersStoreKey = "Mafia_Host_Users";
    private const string PersistSettingsStoreKey = "Mafia_Host_Settings";
    private const string ReplayStoreKey = "Mafia_Host_Replays";
    private readonly Referee referee;
    private readonly PermissionFather panhandler;
    private City city;
    private HostOptions options;
    private LanguageOption language;
    private string navigationPath;
    private List<Replay> replays;

    public Dictionary<string, Color> OperationColors { get; }
    public City City => city;
    public Dictionary<KnownRoleKey, string> KnownRoles { get; }
    public Dictionary<string, string> Messages { get; private set; }

    public HostViewModel(Game game, Referee referee, IOptions<HostOptions> options, PermissionFather panhandler)
    {
        this.game = game;
        this.referee = referee;
        this.panhandler = panhandler;
        this.options = options.Value;
        OperationColors = this.options.Theme.OperationColors.ToDictionary(c => c.Operation, c => c.Color);
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
        await InitSettings();

        replays = await ReadReplays();
        users = await ReadUsers();
        await CreatePresetUsers();
        CreateActiveUsers();
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

    private async Task SaveGameReplay(State state)
    {
        replays.Add(state.Replay);
        await WriteReplace(replays);
        Debug.WriteLine($"[{state.Replay}]");
    }

    private async Task ApplyRatings(State state)
    {
        var ratings = await referee.GetRatings(state.Replay, state.City);
        
        ratings
            .Select(r => (r.rating, user: state.Users0.Single(u => u.Nick == r.nick)))
            .ForEach(v =>
            {
                if (!v.user.Ratings.TryGetValue(state.City.Name, out var ratings))
                {
                    ratings = new();
                    v.user.Ratings.Add(state.City.Name, ratings);
                }

                ratings[state.Replay.Id] = v.rating;
            });

        await WriteUsers();
    }

    private Task<List<Replay>> ReadReplays() => Stores.GetDataByKey<List<Replay>>(ReplayStoreKey);
    private Task WriteReplace(ICollection<Replay> replays) => Stores.SetDataByKey(ReplayStoreKey, replays);
    private Task<PersistSettings> ReadPersistSettings() => Stores.GetDataByKey<PersistSettings>(PersistSettingsStoreKey);
    private Task WritePersistSettings(PersistSettings persistSettings) => Stores.SetDataByKey(PersistSettingsStoreKey, persistSettings);
    private Task<List<User>> ReadUsers() => Stores.GetDataByKey<List<User>>(UsersStoreKey);
    private Task WriteUsers(ICollection<User> users) => Stores.SetDataByKey(UsersStoreKey, users);
}
