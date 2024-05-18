using Host.Libraries;
using Host.Model;
using Mafia.Extensions;
using Mafia.Model;

namespace Host.ViewModel;

public partial class HostViewModel
{
    private const string PersistSettingsSecureKey = "Mafia_Host_PersistSettings";

    private City[] cityMaps;
    private PersistSettings persistSettings;

    public ActiveSettings Settings { get; private set; }
    public string GameClubRules => Messages["GameClubRules"].With($"'{Settings.SelectedClub}'");
    public string GameClubRuleDetails => Messages["GameClubRuleDetails"].With($"'{Settings.SelectedClub}'");

    string GetLangFullName(string name) => name switch
    {
        "ru" => "Русский",
        "en" => "English",
        _ => "NotImplemented"
    };

    ActiveClub[] GetClubs(string lang) => cityMaps.Where(m => m.Language == lang).Select(m => new ActiveClub() { Name = m.Name }).ToArray();

    private void OnSettingsChange(string name)
    {
        if (Settings == null)
            return;

        if (name == nameof(ActiveSettings.SelectedLanguage))
        {
            language = options.Languages.Single(l => l.Name == Settings.SelectedLanguage);
            Messages = language.Messages.ToDictionary(v => v.Name, v => v.Text);
            Settings.Clubs = GetClubs(Settings.SelectedLanguage);
            Settings.GameCommonRulesDescription = Messages["GameCommonRulesDescription"];
            Settings.SelectedClub = persistSettings.Club ?? Settings.Clubs[0].Name;
        }

        if (name == nameof(ActiveSettings.SelectedClub))
        {
            city = cityMaps.Single(m => m.Name == Settings.SelectedClub && m.Language == Settings.SelectedLanguage);
            Settings.GameClubRules = city.Description.SJoin(" ");
            Settings.GameClubRuleDetails = city.Rules.Where(r => r.Accepted).Select(r => r.Description).SJoin("\r\n");

            Changed(nameof(GameClubRules), nameof(GameClubRuleDetails), nameof(City));

            persistSettings.Lang = Settings.SelectedLanguage;
            persistSettings.Club = Settings.SelectedClub;
            Task.Run(async () => await WritePersistSettings(persistSettings)).Wait();
        }
    }

    ActiveLang[] GetLanguages() => cityMaps.Select(m => m.Language).Distinct()
        .OrderBy(v => v).Select(name => new ActiveLang { Name = name, FullName = GetLangFullName(name) }).ToArray();

    private async Task InitSettings()
    {
        persistSettings = await ReadPersistSettings();

        Settings = new ActiveSettings(OnSettingsChange)
        {
            Languages = GetLanguages(),
        };

        Settings.SelectedLanguage = persistSettings.Lang ?? "ru";
    }

    private async Task LoadCityMaps()
    {
        List<City> maps = new();

        foreach(var map in options.Maps)
        {
            using Stream mafiaStream = await FileSystem.Current.OpenAppPackageFileAsync(Path.Combine(HostValues.MapFolder, map.File));
            using TextReader textReader = new StreamReader(mafiaStream);
            var json = textReader.ReadToEnd();
            var city = json.FromJson<City>();
            city.Pic = map.Pic;

            maps.Add(city!);
        }

        cityMaps = maps.ToArray();
    }

    private async Task<PersistSettings> ReadPersistSettings()
    {
        var json = await SecureStorage.Default.GetAsync(PersistSettingsSecureKey);

        if (!json.HasText())
            return new PersistSettings();

        return json.FromJson<PersistSettings>();
    }

    private async Task WritePersistSettings(PersistSettings persistSettings)
    {
        await SecureStorage.Default.SetAsync(PersistSettingsSecureKey, persistSettings.ToJson());
    }
}
