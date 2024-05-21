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


    public bool IsAddUserButtonVisible => !AreSelectedOnly;
    public bool IsUsersTabAvailable => ActiveUsers.Count > 0;
    public bool AreSelectedOnly { get => _areSelectedOnly; set { _areSelectedOnly = value; Changed(); ActiveUsers.ForEach(a=>a.RefreshButtons(!value, ActiveUsers)); Changed(nameof(FilteredActiveUsers), nameof(IsAddUserButtonVisible)); } }

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
        var aUser = GetActiveUser(user);
        ActiveUsers.Add(aUser);
        aUser.RefreshButtons(!AreSelectedOnly, ActiveUsers);

        if (ActiveUsers.Count > 1)
            ActiveUsers[^2].RefreshButtons(!AreSelectedOnly, ActiveUsers);

        WriteUsersInTime();
        Changed(nameof(FilteredActiveUsers), nameof(PlayerInfo), nameof(PlayerRoleInfo));
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

    private async Task RefreshFilteredActiveUsers()
    {
        ActiveUsers.ForEach(a => a.RefreshButtons(!AreSelectedOnly, ActiveUsers));
        Changed(nameof(FilteredActiveUsers));
    }

    private Task WriteUsers() => WriteUsers(users);
    private void WriteUsersInTime() => Runs.FirstInTime(WriteUsers, TimeSpan.FromMilliseconds(500));

    private void OnActiveUserChange(string name, ActiveUser activeUser)
    {
        if (name == "Up")
        {
            var index = ActiveUsers.IndexOf(activeUser);

            (ActiveUsers[index - 1], ActiveUsers[index]) = (ActiveUsers[index], ActiveUsers[index - 1]);
            (users[index - 1], users[index]) = (users[index], users[index - 1]);

            Runs.FirstInTime(RefreshFilteredActiveUsers, TimeSpan.FromMilliseconds(10));
        }

        if (name == "Down")
        {
            var index = ActiveUsers.IndexOf(activeUser);

            (ActiveUsers[index + 1], ActiveUsers[index]) = (ActiveUsers[index], ActiveUsers[index + 1]);
            (users[index + 1], users[index]) = (users[index], users[index + 1]);

            Runs.FirstInTime(RefreshFilteredActiveUsers, TimeSpan.FromMilliseconds(10));
        }

        WriteUsersInTime();

        if (name == nameof(ActiveUser.IsSelected))
        {
            Changed(nameof(PlayerInfo), nameof(PlayerRoleInfo));
            
            if (AreSelectedOnly)
                Runs.FirstInTime(RefreshFilteredActiveUsers, TimeSpan.FromMilliseconds(100));
        }                
    }
}
