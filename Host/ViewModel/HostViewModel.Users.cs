﻿using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using Host.Extensions;
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

    private void OnTabUsersNavigated()
    {

    }
    
    public ICommand AddUserCommand => new Command(() =>
    {
        var user = new User { Nick = $"Nick{ActiveUsers.Count + 1}", LastPlay = DateTime.Today };
        users.Add(user);

        ActiveUsers.Add(new ActiveUser(user, OnActiveUserChange, nameof(ActiveUsers))
        {
            NickColorSilent = options.CityColor,
            IsSelectedSilent = true,
        });

        Changed(nameof(FilteredActiveUsers));
    });

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

        ActiveUsers = users.OrderBy(u => u.Nick).Take(options.PresetPlayerCount).Select(GetActiveUser).ToList();
    }

    private ActiveUser GetActiveUser(User user, int i) => new ActiveUser(user, OnActiveUserChange, nameof(ActiveUsers)) 
    {
        NickColorSilent = options.CityColor,
        IsSelectedSilent = i < options.PresetPlayerSelectedCount
    };

    private void OnActiveUserChange(string name, ActiveUser activeUser)
    {
        async Task Changed_FilteredActiveUsers() => Changed(nameof(FilteredActiveUsers));
        Task WriteUsers_users() => WriteUsers(users);

        Runs.FirstInTime(WriteUsers_users, TimeSpan.FromMilliseconds(500));

        if (name == nameof(ActiveUser.IsSelected))
        {
            Changed(nameof(PlayerInfo));
            Runs.FirstInTime(Changed_FilteredActiveUsers, TimeSpan.FromMilliseconds(1000));
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
