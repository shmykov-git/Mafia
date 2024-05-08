using System.Diagnostics;
using System.Windows.Input;
using Host.Model;
using Mafia.Extensions;

namespace Host.ViewModel;

public partial class HostViewModel
{
    private TaskCompletionSource? interactor = null;
    private bool onActivePlayerSilent = false;
    private string _hostHint;
    private string _text;
    private string _playerInfo;

    public string HostHint { get => _hostHint; set { _hostHint = value; Changed(nameof(HostHint)); } }
    public string Text { get => _text; set { _text = value; Changed(); } } // todo: temp
    public string PlayerInfo { get => _playerInfo; set { _playerInfo = value; Changed(); } }

    public ActiveRole[] ActiveRoles { get; private set; }
    public ActivePlayer[] ActivePlayers { get; private set; }


    public ICommand StartNewGameCommand => new Command(async () =>
    {
        await StartNewGame();
        await Shell.Current.GoToAsync("//pages/GameView");
    });


    public ICommand ContinueCommand => new Command(async () =>
    {
        await Task.Delay(20);
        Continue();
    }, () => game.IsActive);


    private async Task StartNewGame()
    {
        // stop previous game execution task line
        var stopTask = game.Stop();
        Continue();
        await stopTask;

        // setup game variant
        var seed = this.seed.HasValue ? this.seed.Value + 1 : options.FirstSeed;
        ChangeSeed(seed);

        Text = "";
        WriteLine($"\r\n'{city.Name}' game {seed}");

        // start new game execution task line
        game.Start().ContinueWith(_ => RefreshCommands()).NoWait();

        // as new line is ready refresh all 
        await Task.Delay(50);
        RefreshCommands();
    }


    private async Task Interact(string message)
    {
        if (game.Stopping)
            return;

        HostHint = message;

        while (interactor != null)
        {
            Debug.WriteLine("warn: async many lines error");
            await Task.Delay(10);
        }

        interactor = new TaskCompletionSource();
        await interactor.Task;
        interactor = null;
    }


    private void Continue() => interactor?.SetResult();
    private void RefreshCommands() => GetType().GetProperties().Where(p => p.PropertyType == typeof(ICommand)).ForEach(p => Changed(p.Name));

    // todo: temp
    private void WriteLine(string text)
    {
        Text += $"{text}\r\n";
    }

    private void OnActiveRoleChange(string? name = null)
    {
        if (ActiveRoles == null)
            return;

        var count = ActiveRoles.Where(r => r.IsSelected).Sum(r => r.Count);
        PlayerInfo = $"Players: {count}";
    }

    private void OnActivePlayerChange(string? name = null)
    {
        if (!onActivePlayerSilent)
        {
            onActivePlayerSilent = true;

            if (ActivePlayers.Where(p => p.IsSelected).Any())
            {
                ActivePlayers.Where(p => !p.IsSelected).ForEach(p => p.IsEnabled = false);
            }
            else
            {
                ActivePlayers.ForEach(p => p.IsEnabled = true);
            }

            onActivePlayerSilent = false;
        }
    }
}
