using System.Collections.ObjectModel;
using System.Windows.Input;
using Host.Extensions;
using Host.Model;
using Mafia.Extensions;
using Mafia.Model;

namespace Host.ViewModel;

public partial class HostViewModel
{
    private const string UsersSecureKey = "Mafia_Host_Users";
    private List<User> users;
    
    private ObservableCollection<ActiveUser> _activeUsers = [];
    public ObservableCollection<ActiveUser> ActiveUsers { get => _activeUsers; set { _activeUsers = value; ChangedSilently(); Changed(nameof(IsUsersTabAvailable)); } }

    public bool IsUsersTabAvailable => ActiveUsers.Count > 0;

    private void OnTabUsersNavigated()
    {

    }

    public ICommand SelectRolesCommand => new Command(async () =>
    {
        await StopCurrentGame();
        InitActiveRoles();
        await Shell.Current.GoToAsync("//pages/StartGameView");
    });

    private async Task InitDatabaseUsers()
    {
        users = await ReadUsers();
        
        if (users.Count < options.PresetPlayerCount)
        {
            var lastPlay = DateTime.Now.Date;
            var newUsers = Enumerable.Range(users.Count + 1, options.PresetPlayerCount - users.Count + 1).Select(i => new User { Nick = $"Nick{i}", LastPlay = lastPlay }).ToArray();
            users.AddRange(newUsers);
            await WriteUsers(users);
        }

        ActiveUsers = users.OrderBy(u => u.Nick).Take(options.PresetPlayerCount).Select(GetActiveUser).ToObservableCollection();
        
        ShowPlayerInfo();
    }

    private ActiveUser GetActiveUser(User user, int i) => new ActiveUser(user, onActiveUserChange, nameof(ActiveUsers)) 
    {
        NickColorSilent = options.CityColor,
        IsSelectedSilent = i < options.PresetPlayerSelectedCount
    };

    private void onActiveUserChange(string name)
    {
        ShowPlayerInfo();
    }

    private void ShowPlayerInfo()
    {
        var count = ActiveUsers.Where(r => r.IsSelected).Count();
        PlayerInfo = Messages["PlayerCountInfo"].With(count);
    }

    private async Task<List<User>> ReadUsers()
    {
        var json = await SecureStorage.Default.GetAsync(UsersSecureKey);

        if (!json.HasText())
            return [];

        return json.FromJson<List<User>>();
    }

    private async Task WriteUsers(ICollection<User> users)
    {
        await SecureStorage.Default.SetAsync(UsersSecureKey, users.ToJson());
    }
}
