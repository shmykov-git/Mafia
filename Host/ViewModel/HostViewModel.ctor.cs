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
public partial class HostViewModel : NotifyPropertyChanged
{
    private Random rnd;
    private readonly Game game;
    private readonly City city;
    private HostOptions options;

    public Dictionary<string, string> Messages { get; }

    public HostViewModel(Game game, City city, IOptions<HostOptions> options)
    {
        rnd = new Random();
        this.game = game;
        this.city = city;
        this.options = options.Value;
        Messages = this.options.Messages.ToDictionary(v => v.Name, v => v.Text);

        InitActiveRoles();
    }

    private void RefreshCommands() => GetType().GetProperties().Where(p => p.PropertyType == typeof(ICommand)).ForEach(p => Changed(p.Name));
}
