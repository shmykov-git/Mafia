using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Host.Model;
using Mafia;
using Mafia.Extensions;
using Mafia.Libraries;
using Mafia.Model;
using Microsoft.Extensions.Options;

namespace Host.ViewModel;
/// <summary>
/// todo: view code for host
/// </summary>
public partial class HostViewModel : NotifyPropertyChanged, IHost
{
    private Random rnd;
    private readonly Game game;
    private readonly City city;
    private HostOptions options;

    public HostViewModel(Game game, City city, IOptions<HostOptions> options)
    {
        rnd = new Random();
        this.game = game;
        this.city = city;
        this.options = options.Value;

        InitActiveRoles();
    }

    private (string name, int count)[] GetRolesPreset(int n)
    {
        return RoleValues.GetRolesPreset(["DonMafia", "BumMafia", "Maniac", "Commissar", "Doctor"], "Mafia", "Civilian", n, 3.5);
    }

    private void InitActiveRoles()
    {
        ActiveRoles = city.AllRoles()
            .Select(r => (role: r, preset: GetRolesPreset(15).FirstOrDefault(rr => rr.name == r.Name)))
            .Select(v => new ActiveRole(v.role, OnActiveRoleChange) { IsSelected = v.preset.count > 0, Count = v.preset.count > 0 ? v.preset.count : 1 }).ToArray();

        OnActiveRoleChange();
    }


}
