using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Host.Extensions;
using Host.Model;
using Host.Views;
using Mafia;
using Mafia.Extensions;
using Mafia.Libraries;
using Mafia.Model;
using Microsoft.Extensions.Options;

namespace Host.ViewModel;

public partial class HostViewModel : NotifyPropertyChanged
{
    private Random rnd;
    private readonly City city;
    private HostOptions options;

    public Dictionary<string, string> Messages { get; }

    public HostViewModel(Game game, City city, IOptions<HostOptions> options)
    {
        rnd = new Random();
        this.game = game;
        this.city = city;
        this.options = options.Value;
        HintColor = this.options.CityColor;
        SelectedPlayerRoleMessageColor = this.options.CityColor;
        Messages = this.options.Messages.ToDictionary(v => v.Name, v => v.Text);

        Task.Run(InitDatabaseUsers).Wait();
    }

    public ICommand NavigatedCommand => new Command(Shell_Navigated);

    private void Shell_Navigated(object path)
    {
        switch (path)
        {
            case "//pages/UserView":
                OnTabUsersNavigated();
                break;

            case "//pages/StartGameView":
                OnTabStartGameNavigated();
                break;

            case "//pages/GameView":
                OnTabGameNavigated();
                break;
        }
    }

    private void RefreshCommands() => GetType().GetProperties().Where(p => p.PropertyType == typeof(ICommand)).ForEach(p => Changed(p.Name));

    private Color GetOperationColor(string? operation) =>
        options.OperationColors.FirstOrDefault(v => v.Operation == operation)?.Color ??
        options.OperationColors.Single(v => v.Operation == "Unknown").Color;

    private void Log(string text)
    {
        Debug.WriteLine(text);
    }
}
