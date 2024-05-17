namespace Host.Model;

public class ActiveSettings : NotifyPropertyChanged
{
    private string _gameCommonRules;
    private string _gameClubRules;
    private string _gameClubRuleDetails;
    private string[] _languages;
    private string[] _mapNames;

    public required string[] Languages { get => _languages; set { _languages = value; Changed(); } }
    public required string[] MapNames { get => _mapNames; set { _mapNames = value; Changed(); } }
    public required string GameCommonRules { get => _gameCommonRules; set { _gameCommonRules = value; Changed(); } }
    public required string GameClubRules { get => _gameClubRules; set { _gameClubRules = value; Changed(); } }
    public required string GameClubRuleDetails { get => _gameClubRuleDetails; set { _gameClubRuleDetails = value; Changed(); } }
}
