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
    private List<User> users;
    private bool _areSelectedOnly = true;
    
    private List<ActiveUser> _activeUsers = [];
    public List<ActiveUser> ActiveUsers { get => _activeUsers; set { _activeUsers = value; ChangedSilently(); Changed(nameof(IsUsersTabAvailable), nameof(PlayerInfo), nameof(PlayerRoleInfo), nameof(FilteredActiveUsers)); } }
    public IEnumerable<ActiveUser> FilteredActiveUsers => ActiveUsers.Where(u => !AreSelectedOnly || u.IsSelected);

    public bool IsUsersTabAvailable => ActiveUsers.Count > 0;

    public bool AreSelectedOnly { get => _areSelectedOnly; set { _areSelectedOnly = value; Changed(); Changed(nameof(FilteredActiveUsers)); } }

    public string PlayerInfo => Messages["PlayerCountInfo"].With(ActiveUsers.Where(r => r.IsSelected).Count());

    private User CreateDefaultUser(int k) => new User
    {
        Nick = $"Nick{k.ToString().PadLeft(2, '0')}"
    };

    private void CreateActiveUsers()
    {
        ActiveUsers = users.Select(GetActiveUser).ToList();
    }

    private async Task CreatePresetUsers()
    {
        if (users.Count >= options.PresetPlayerCount)
            return;
        var skipSelectedCount = Math.Max(users.Count, options.PresetPlayerSelectedCount);
        var newUsers = Enumerable.Range(users.Count + 1, options.PresetPlayerCount - users.Count).Select(CreateDefaultUser).ToArray();
        
        users.AddRange(newUsers);
        users.Skip(skipSelectedCount).ForEach(u => u.IsSelected = false);

        await WriteUsers(users);
    }

    public ICommand AddUserCommand => new Command(() =>
    {
        var user = CreateDefaultUser(ActiveUsers.Count + 1);
        users.Add(user);
        ActiveUsers.Add(GetActiveUser(user));

        WriteUsersInTime();
        Changed(nameof(FilteredActiveUsers));
    });

    public ICommand SelectRolesCommand => new Command(async () =>
    {
        await StopCurrentGame();
        InitActiveRoles();
        await Shell.Current.GoToAsync(HostValues.RolesView);
    });

    private ActiveUser GetActiveUser(User user) => new ActiveUser(user, OnActiveUserChange, nameof(ActiveUsers)) 
    {
        NickColorSilent = options.Theme.CityColor,
    };

    private Task WriteUsers() => WriteUsers(users);
    private void WriteUsersInTime() => Runs.FirstInTime(WriteUsers, TimeSpan.FromMilliseconds(500));

    private void OnActiveUserChange(string name, ActiveUser activeUser)
    {
        async Task Changed_FilteredActiveUsers() => Changed(nameof(FilteredActiveUsers));
        WriteUsersInTime();

        if (name == nameof(ActiveUser.IsSelected))
        {
            Changed(nameof(PlayerInfo), nameof(PlayerRoleInfo));
            Runs.FirstInTime(Changed_FilteredActiveUsers, TimeSpan.FromMilliseconds(500));
        }                
    }
}
