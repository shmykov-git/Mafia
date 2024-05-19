using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using Host.Extensions;
using Host.Libraries;
using Host.Model;
using Mafia.Extensions;
using Mafia.Libraries;
using Mafia.Model;

namespace Host.ViewModel;

public partial class HostViewModel
{
    private const string UsersSecureKey = "Mafia_Host_Users";

    private List<User> users;
    private bool _areSelectedOnly = true;
    
    private List<ActiveUser> _activeUsers = [];
    public List<ActiveUser> ActiveUsers { get => _activeUsers; set { _activeUsers = value; ChangedSilently(); Changed(nameof(IsUsersTabAvailable), nameof(PlayerInfo), nameof(PlayerRoleInfo), nameof(FilteredActiveUsers)); } }
    public IEnumerable<ActiveUser> FilteredActiveUsers => ActiveUsers.Where(u => !AreSelectedOnly || u.IsSelected);

    public bool IsUsersTabAvailable => ActiveUsers.Count > 0;

    public bool AreSelectedOnly { get => _areSelectedOnly; set { _areSelectedOnly = value; Changed(); Changed(nameof(FilteredActiveUsers)); } }

    public string PlayerInfo => Messages["PlayerCountInfo"].With(ActiveUsers.Where(r => r.IsSelected).Count());

    public ICommand AddUserCommand => new Command(() =>
    {
        var user = new User { Nick = $"Nick{ActiveUsers.Count + 1}" };
        users.Add(user);

        ActiveUsers.Add(new ActiveUser(user, OnActiveUserChange, nameof(ActiveUsers))
        {
            NickColorSilent = options.Theme.CityColor,
            IsSelectedSilent = true,
        });

        Changed(nameof(FilteredActiveUsers));
    });

    public ICommand SelectRolesCommand => new Command(async () =>
    {
        await StopCurrentGame();
        InitActiveRoles();
        await Shell.Current.GoToAsync(HostValues.RolesView);
    });

    private ActiveUser GetActiveUser(User user, int i) => new ActiveUser(user, OnActiveUserChange, nameof(ActiveUsers)) 
    {
        NickColorSilent = options.Theme.CityColor,
        IsSelectedSilent = i < options.PresetPlayerSelectedCount
    };

    private void OnActiveUserChange(string name, ActiveUser activeUser)
    {
        async Task Changed_FilteredActiveUsers() => Changed(nameof(FilteredActiveUsers));
        Task WriteUsers_users() => WriteUsers(users);

        Runs.FirstInTime(WriteUsers_users, TimeSpan.FromMilliseconds(500));

        if (name == nameof(ActiveUser.IsSelected))
        {
            Changed(nameof(PlayerInfo), nameof(PlayerRoleInfo));
            Runs.FirstInTime(Changed_FilteredActiveUsers, TimeSpan.FromMilliseconds(500));
        }                
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
