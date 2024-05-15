using System.Diagnostics;
using System.Windows.Input;
using Host.Model;
using Mafia.Extensions;
using Mafia.Libraries;
using Mafia.Model;

namespace Host.ViewModel;

public partial class HostViewModel
{

    private (string name, int count)[] GetRolesPreset(int n)
    {
        return RoleValues.GetRolePreset(options.RolePreset, n);
    }

    public bool IsStartGameTabAvailable => ActiveRoles.Length > 0;

    public string PlayerRoleInfo => Messages["SelectPlayerRoles"].With(ActiveUsers.Where(r => r.IsSelected).Count());


    public ActiveRole[] ActiveRolesSilent = [];
    public ActiveRole[] ActiveRoles { get => ActiveRolesSilent; set { ActiveRolesSilent = value; ChangedSilently(); Changed(nameof(IsStartGameTabAvailable)); } }

    private Role[] GetSelectedMultipliedRoles() => ActiveRoles.Where(r => r.IsSelected).SelectMany(r => Enumerable.Range(0, r.Count).Select(_ => r.Role)).ToArray();
    private Role[] GetSelectedRoles() => ActiveRoles.Where(r => r.IsSelected).Select(r => r.Role).ToArray();

    private void OnTabStartGameNavigated()
    {

    }

    private void InitActiveRoles()
    {
        var n = ActiveUsers.Count(u => u.IsSelected);
        var preset = RoleValues.GetRolePreset(options.RolePreset, n);

        ActiveRoles = city.AllRoles()
            .Select(r => (role: r, preset: preset.FirstOrDefault(rr => rr.name == r.Name)))
            .Select(v => new ActiveRole(v.role, OnActiveRoleChange, nameof(ActiveRoles)) 
            { 
                RoleColorSilent = GetRoleColor(v.role.Name),
                IsSelectedSilent = v.preset.count > 0, 
                CountSilent = v.preset.count > 0 ? v.preset.count : 1 
            }).ToArray();
    }
    
    public bool AreRolesValid() => ActiveRoles.Where(a => a.IsCounter).Min(a => a.Count) >= 0;

    private void OnActiveRoleChange(string name, ActiveRole activeRole)
    {
        var n = ActiveUsers.Count(u => u.IsSelected);
        var k = ActiveRoles.Where(r => r.IsSelected).Sum(r => r.Count);

        if (n == k)
            return;

        var update = AreRolesValid()
            ? ActiveRoles.Where(a => a != activeRole).Where(a => a.IsCounter).MaxBy(a => a.Count)
            : ActiveRoles.Where(a => a != activeRole).Where(a => a.IsCounter).MinBy(a => a.Count);

        update!.Count += (n - k);

        Changed(nameof(StartNewGameCommand));
    }

    public ICommand StartNewGameCommand => new Command(async () =>
    {
        await StartNewGame();
        
        for(var i = 0; i<100;i++)
        {
            await Task.Delay(10);
            
            if (IsGameTabAvailable)
                break;
        }

        await Shell.Current.GoToAsync("//pages/GameView");
    }, AreRolesValid);

    // todo: ask user to restart the game
    private async Task StopCurrentGame()
    {
        if (IsGameOn)
        {
            // stop previous game execution task line
            var stopTask = game.Stop();
            Continue();
            await stopTask;
        }

        ActivePlayers = [];
    }

    private async Task StartNewGame()
    {
        await StopCurrentGame();

        // setup game variant
        var seed = this.seed.HasValue ? this.seed.Value + 1 : options.FirstSeed;
        ChangeSeed(seed);

        Log("");
        Log($"'{city.Name}' game {seed}");

        // start new game execution task line
        game.Start().ContinueWith(_ => RefreshCommands()).NoWait();

        // as new line is ready refresh all 
        await Task.Delay(50);
        RefreshCommands();
    }
}
