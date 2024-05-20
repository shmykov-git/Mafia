namespace Host.Model;

public class ActiveSettings : NotifyPropertyChanged
{
    private string _selectedLanguage;
    private string _selectedClub;

    private string _gameCommonRules;
    private string _gameClubRules;
    private ActiveLang[] _langs;
    private ActiveClub[] _clubs;
    private ActiveRule[] _rules;

    public ActiveSettings(Action<string> onChange)
    {
        Subscribe(onChange);
    }

    private void CheckLang() => Languages.Single(l => l.Name == SelectedLanguage).IsChecked = true;
    private void CheckClub() => Clubs.Single(l => l.Name == SelectedClub).IsChecked = true;

    public string SelectedLanguage { get => _selectedLanguage; set { _selectedLanguage = value; Changed(); CheckLang(); } }
    public string SelectedClub { get => _selectedClub; set { _selectedClub = value; Changed(); CheckClub(); } }

    public ActiveLang[] Languages { get => _langs; set { _langs = value; Changed(); } }
    public ActiveClub[] Clubs { get => _clubs; set { _clubs = value; Changed(); } }
    public string GameCommonRulesDescription { get => _gameCommonRules; set { _gameCommonRules = value; Changed(); } }
    public string GameClubRules { get => _gameClubRules; set { _gameClubRules = value; Changed(); } }

    public ActiveRule[] Rules { get => _rules; set { _rules = value; Changed(); } }
}
