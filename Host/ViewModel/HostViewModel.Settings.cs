using Host.Libraries;
using Host.Model;
using Mafia.Extensions;
using Mafia.Model;

namespace Host.ViewModel;

public partial class HostViewModel : ICity
{
    private City[] cityMaps;
    public City City => city;

    public ActiveSettings Settings { get; private set; }

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
            Settings.GameCommonRules = Messages["GameCommonRules"];
            Settings.SelectedClub = Settings.Clubs[0].Name;
        }

        if (name == nameof(ActiveSettings.SelectedClub))
        {
            city = cityMaps.Single(m => m.Name == Settings.SelectedClub && m.Language == Settings.SelectedLanguage);
            Settings.GameClubRules = city.Description.SJoin(" ");
            Settings.GameClubRuleDetails = city.Rules.Where(r => r.Accepted).Select(r => r.Description).SJoin("\r\n");
        }
    }

    ActiveLang[] GetLanguages() => cityMaps.Select(m => m.Language).Distinct()
        .OrderBy(v => v).Select(name => new ActiveLang { Name = name, FullName = GetLangFullName(name) }).ToArray();

    private void InitSettings()
    {
        Settings = new ActiveSettings(OnSettingsChange)
        {
            Languages = GetLanguages(),
        };

        Settings.SelectedLanguage = "ru";
    }

    private async Task LoadCityMaps()
    {
        List<City> maps = new();

        foreach(var mapFile in options.Maps)
        {
            using Stream mafiaStream = await FileSystem.Current.OpenAppPackageFileAsync(Path.Combine(HostValues.MapFolder, mapFile));
            using TextReader textReader = new StreamReader(mafiaStream);
            var json = textReader.ReadToEnd();
            var city = json.FromJson<City>();

            maps.Add(city!);
        }

        cityMaps = maps.ToArray();
    }
}
