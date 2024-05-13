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
        return RoleValues.GetRolesPreset(["Дон", "Бомж", "Маньяк", "Комиссар", "Доктор"], "Мафия", "Мирный", n, 3.5);
        //return RoleValues.GetRolesPreset(["DonMafia", "BumMafia", "Maniac", "Commissar", "Doctor"], "Mafia", "Civilian", n, 3.5);
    }

    public bool IsStartGameTabAvailable => ActiveRoles.Length > 0;


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

        ActiveRoles = city.AllRoles()
            .Select(r => (role: r, preset: GetRolesPreset(n).FirstOrDefault(rr => rr.name == r.Name)))
            .Select(v => new ActiveRole(v.role, OnActiveRoleChange, nameof(ActiveRoles)) 
            { 
                RoleColorSilent = GetRoleColor(v.role.Name),
                IsSelectedSilent = v.preset.count > 0, 
                CountSilent = v.preset.count > 0 ? v.preset.count : 1 
            }).ToArray();
    }

    private void OnActiveRoleChange(string name, ActiveRole activeRole)
    {
        var n = ActiveUsers.Count(u => u.IsSelected);
        var k = ActiveRoles.Where(r => r.IsSelected).Sum(r => r.Count);

        if (n == k)
            return;

        switch (name)
        {
            case nameof(ActiveRole.IsSelected):
                ActiveRoles.Where(a => a.IsCounter).MaxBy(a => a.Count).Count += n - k;
                break;

            case nameof(ActiveRole.Count):
                ActiveRoles.Where(a => a != activeRole).Where(a => a.IsCounter).MaxBy(a => a.Count).Count += n - k;
                break;                
        }
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
    });

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
