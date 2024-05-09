using System.Diagnostics;
using System.Windows.Input;
using Host.Model;
using Mafia.Extensions;
using Mafia.Model;
using Newtonsoft.Json;

namespace Host.ViewModel;
public partial class HostViewModel
{
    private TaskCompletionSource? hostWaiter = null;
    private bool onActivePlayerSilent = false;
    private string _hostHint;
    private string _playerInfo;
    private ActivePlayer[] _activePlayers;
    private Interaction? interaction = null;

    public string HostHint { get => _hostHint; set { _hostHint = value; Changed(nameof(HostHint)); } }
    public string PlayerInfo { get => _playerInfo; set { _playerInfo = value; Changed(); } }
    public ActivePlayer[] ActivePlayers { get => _activePlayers; set { _activePlayers = value; Changed(); } }

    public ICommand ContinueCommand => new Command(async () =>
    {
        await Task.Delay(20);
        Continue();
    }, () => game.IsActive && (interaction == null || ActivePlayers.Count(p => p.IsSelected).Between(interaction.Selection)));


    private async Task<InteractionResult> Interact(Interaction interaction)
    {
        Debug.WriteLine(interaction.Selection);

        if (game.Stopping)
            return new InteractionResult();

        this.interaction = interaction;
        Log(interaction.Message);
        HostHint = interaction.Message;

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

    // todo: temp
    private void Log(string text)
    {
        Debug.WriteLine(text);
    }

    private void SinchActivePlayers(State state)
    {
        ActivePlayers = ActivePlayers.Where(p => state.Players.Contains(p.Player)).ToArray();
    }

    private void PrepareActivePlayers(Interaction interaction)
    {
        var playerColors = interaction.State.Players0.GroupBy(p => p.Group.Name).OrderBy(gv => gv.Key)
            .SelectMany((gv, i) => gv.Select(p => (p, i)))
            .ToDictionary(v => v.p, v => options.GroupColors[v.i % options.GroupColors.Length]);

        UpdateActivePlayers(p =>
        {
            p.TextColor = playerColors[p.Player];
            p.Operation = interaction.Killed.Contains(p.Player) ? "killed" : "";
            p.IsEnabled = !interaction.Except.Contains(p.Player);
            p.IsSelected = false;
        }, silent:false);
    }

    private void UpdateActivePlayers(Interaction interaction)
    {
        var isEnabled = ActivePlayers.Count(a => a.IsSelected).Between(0, interaction.Selection.to - 1);

        UpdateActivePlayers(p =>
        {
            p.IsEnabled = isEnabled && !interaction.Except.Contains(p.Player);
        }, p => !p.IsSelected, false);

        Changed(nameof(ContinueCommand));
    }

    private void UpdateActivePlayers(Action<ActivePlayer> action, Func<ActivePlayer, bool>? predicate = null, bool silent = true)
    {
        if (!onActivePlayerSilent)
        {
            onActivePlayerSilent = true;

            foreach (var activePlayer in ActivePlayers)
            {
                if (predicate?.Invoke(activePlayer) ?? true)
                    action(activePlayer);
            }

            if (silent)
                ActivePlayers = ActivePlayers.ToArray();

            onActivePlayerSilent = false;
        }
    }

    private void OnActivePlayerChange(string? name = null)
    {
        if (interaction != null)
            UpdateActivePlayers(interaction);
    }
}
