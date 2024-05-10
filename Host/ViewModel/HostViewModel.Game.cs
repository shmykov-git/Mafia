using System;
using System.Diagnostics;
using System.Windows.Input;
using Host.Model;
using Mafia;
using Mafia.Extensions;
using Mafia.Model;
using Newtonsoft.Json;

namespace Host.ViewModel;
public partial class HostViewModel
{
    private readonly Game game;
    private TaskCompletionSource? hostWaiter = null;
    private string _hostHint;
    private string _playerInfo;
    private ActivePlayer[] _activePlayers;
    private Interaction? interaction = null;

    public string HostHint { get => _hostHint; set { _hostHint = value; Changed(); } }
    public string PlayerInfo { get => _playerInfo; set { _playerInfo = value; Changed(); } }
    public ActivePlayer[] ActivePlayers { get => _activePlayers; set { _activePlayers = value; ChangedSilently(); } }

    public ICommand ContinueCommand => new Command(async () =>
    {
        await Task.Delay(20);
        Continue();
    }, () => game.IsActive && (interaction == null || ActivePlayers.Count(p => p.IsSelected).Between(interaction.Selection)));


    private async Task<InteractionResult> Interact(Interaction interaction)
    {
        if (game.Stopping)
            return new InteractionResult();

        this.interaction = interaction;

        var message = string.Format(Messages[interaction.Name], interaction.Args ?? []);

        Log(message);
        HostHint = message;

        PrepareActivePlayers(interaction);
        UpdateActivePlayers(interaction);
        Changed(nameof(ContinueCommand));

        // warn when game started many times without completion or other async errors
        while (hostWaiter != null)
        {
            Debug.WriteLine("error: many task lines");
            await Task.Delay(10);
        }

        // wait for the host interacts with game team (with real peoples)
        hostWaiter = new TaskCompletionSource();
        await hostWaiter.Task;
        hostWaiter = null;

        var result = new InteractionResult() 
        {
            Selected = ActivePlayers.Where(p => p.IsSelected).Select(p => p.Player).ToArray()
        };

        if (result.IsSkipped)
        {
            Log($"{(interaction.WithCity ? "City" : interaction.Player)}, skip selection");
        }
        else
        {
            Log($"{(interaction.WithCity ? "City" : interaction.Player)} --> {result.Selected.SJoin(", ")}");
        }

        this.interaction = null;
        Changed(nameof(ContinueCommand));

        return result;
    }


    private void Continue() => hostWaiter?.SetResult();

    private void PrepareActivePlayers(Interaction interaction)
    {
        UpdateActivePlayers(p =>
        {
            p.Operation = interaction.Killed.Contains(p.Player) ? Messages["killed"] : "";
            p.IsEnabled = !interaction.Except.Contains(p.Player);
            p.IsSelected = false;
        });
    }

    private void UpdateActivePlayers(Interaction interaction)
    {
        var isEnabled = ActivePlayers.Count(a => a.IsSelected).Between(0, interaction.Selection.to - 1);

        UpdateActivePlayers(p =>
        {
            p.IsEnabled = isEnabled && !interaction.Except.Contains(p.Player);
        }, p => !p.IsSelected);

        Changed(nameof(ContinueCommand));
    }

    private void UpdateActivePlayers(Action<ActivePlayer> action, Func<ActivePlayer, bool>? predicate = null)
    {
        DoSilent(nameof(ActivePlayers), () =>
        {
            foreach (var activePlayer in ActivePlayers)
            {
                if (predicate?.Invoke(activePlayer) ?? true)
                    action(activePlayer);
            }
        });
    }

    private void OnActivePlayerChange(string? name = null)
    {
        if (interaction == null)
            return;

        UpdateActivePlayers(interaction);
    }
}
