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

    public ActiveRole[] ActiveRolesSilent;
    public ActiveRole[] ActiveRoles { get => ActiveRolesSilent; set { ActiveRolesSilent = value; ChangedSilently(); } }

    private void InitActiveRoles()
    {
        ActiveRoles = city.AllRoles()
            .Select(r => (role: r, preset: GetRolesPreset(15).FirstOrDefault(rr => rr.name == r.Name)))
            .Select(v => new ActiveRole(v.role, OnActiveRoleChange) { IsSelected = v.preset.count > 0, Count = v.preset.count > 0 ? v.preset.count : 1 }).ToArray();

        OnActiveRoleChange();
    }

    private void OnActiveRoleChange(string? name = null)
    {
        if (IsSilent(nameof(ActiveRoles)) || ActiveRoles == null)
            return;
        
        var count = ActiveRoles.Where(r => r.IsSelected).Sum(r => r.Count);
        PlayerInfo = Messages["PlayerCountInfo"].With(count);
    }

    public ICommand StartNewGameCommand => new Command(async () =>
    {
        await StartNewGame();
        await Shell.Current.GoToAsync("//pages/GameView");
    });

    private async Task StartNewGame()
    {
        // stop previous game execution task line
        var stopTask = game.Stop();
        Continue();
        await stopTask;

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
