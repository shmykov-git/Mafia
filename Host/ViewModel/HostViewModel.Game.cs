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
    private string _hostWakeUpHint;
    private bool _isVisibleHostWakeUpHint;
    private bool _isActivePlayerRoleVisible;
    private string _playerInfo;
    private ActiveRole[] _activePlayerRoles;
    private ActivePlayer[] _activePlayers;
    private Interaction? prevInteraction = null;
    private Interaction? interaction = null;
    private Color _hintColor;

    public Color HintColor { get => _hintColor; set { _hintColor = value; Changed(); } }
    public bool IsVisibleHostWakeUpHint { get => _isVisibleHostWakeUpHint; set { _isVisibleHostWakeUpHint = value; Changed(); } }
    public string HostWakeUpHint { get => _hostWakeUpHint; set { _hostWakeUpHint = value; Changed(); } }
    public string HostHint { get => _hostHint; set { _hostHint = value; Changed(); } }
    public string PlayerInfo { get => _playerInfo; set { _playerInfo = value; Changed(); } }
    public ActivePlayer[] ActivePlayers { get => _activePlayers; set { _activePlayers = value; ChangedSilently(); } }
    public ActiveRole[] ActivePlayerRoles { get => _activePlayerRoles; set { _activePlayerRoles = value; ChangedSilently(); } }
    public bool IsActivePlayerRoleVisible { get => _isActivePlayerRoleVisible; set { _isActivePlayerRoleVisible = value; Changed(); } }

    public ICommand ContinueCommand => new Command(async () =>
    {
        await Task.Delay(20);
        Continue();
    }, () => game.IsActive && (interaction == null || ActivePlayers.Count(p => p.IsSelected).Between(interaction.Selection)));

    private void ShowWakeUpMessage()
    {
        if (interaction.State.IsDay || interaction.Player == null)
            return;
        
        HintColor = ActivePlayers.Single(p => p.Player == interaction.Player).RoleColor;

        var isGroup = interaction.Player.Group.AllRoles().Count() == 1;

        var wakeUpMessage = isGroup
            ? Messages["PlayerWakeUp"].With(interaction.Player.Role.Name)
            : Messages["GroupWakeUp"].With(interaction.Player.Group.Name, interaction.State.GetGroupActivePlayers(interaction.Player.Group).Select(p => p.Role.Name).SJoin(", "));

        IsVisibleHostWakeUpHint = true;
        HostWakeUpHint = wakeUpMessage;        
    }

    private async Task ShowFallAsleepMessage()
    {
        if (prevInteraction?.Player?.Group == null || prevInteraction.Player.Group == interaction.Player?.Group)
            return;
        
        var isGroup = prevInteraction.Player.Group.AllRoles().Count() == 1;

        var fallAsleepMessage = isGroup
            ? Messages["PlayerFallAsleep"].With(prevInteraction.Player.Role.Name)
            : Messages["GroupFallAsleep"].With(prevInteraction.Player.Group.Name);

        HintColor = ActivePlayers.Single(p => p.Player == prevInteraction.Player).RoleColor;
        HostHint = fallAsleepMessage;
        await Task.Delay(options.HostFallAsleepMessageDelay);        
    }

    private void ShowHostMessage()
    {
        var message = Messages[interaction.Name].With(interaction.Args);
        HostHint = message;
        Log(message);
    }

    private async Task WaitForHostInteraction()
    {
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
    }

    private void CleanUpInteraction()
    {
        IsVisibleHostWakeUpHint = false;
        HostWakeUpHint = "";
        HintColor = options.CityColor;
    }

    private async Task<InteractionResult> Interact(Interaction interaction)
    {
        if (game.Stopping)
            return new InteractionResult();

        this.interaction = interaction;

        await ShowFallAsleepMessage();
        ShowWakeUpMessage();
        ShowHostMessage();

        PrepareActivePlayers(interaction);

        await WaitForHostInteraction();

        var result = new InteractionResult() 
        {
            Selected = ActivePlayers.Where(p => p.IsSelected).ToArray()
        };



        CleanUpInteraction();

        if (result.IsSkipped)
            Log($"{(interaction.WithCity ? "City" : interaction.Player)}, skip selection");
        else
            Log($"{(interaction.WithCity ? "City" : interaction.Player)} --> {result.Selected.SJoin(", ")}");

        prevInteraction = interaction;
        this.interaction = null;

        return result;
    }


    private void Continue() => hostWaiter?.SetResult();

    private void PrepareActivePlayerRoles(Interaction interaction)
    {
        string[] except = ActivePlayers.Where(p => p.Player != null).Select(p => p.Player.Role.Name)
            .Concat(interaction.State.LatestDayNews.Killed.Select(p => p.Role.Name)).ToArray();
        
        ActivePlayerRoles = ActiveRoles.Where(r=>r.IsSelected).Where(r => !except.Contains(r.Role.Name))
            .Select(r => new ActiveRole(r.Role, OnActivePlayerRoleChange, nameof(ActivePlayerRoles)) { RoleColorSilent = r.RoleColor }).ToArray();
    }

    private void OnActivePlayerRoleChange(string name, ActiveRole role)
    {
        var isEnabled = ActivePlayerRoles.Count(a => a.IsSelected) == 0;

        UpdateActivePlayerRoles(role =>
        {
            role.IsEnabled = isEnabled;
        }, role => !role.IsSelected);
    }

    private void UpdateActivePlayerRoles(Action<ActiveRole> action, Func<ActiveRole, bool>? predicate = null)
    {
        foreach (var activeRole in ActivePlayerRoles)
            if (predicate?.Invoke(activeRole) ?? true)
                activeRole.DoSilent(nameof(ActivePlayerRoles), () => action(activeRole));
    }


    private void PrepareActivePlayers(Interaction interaction)
    {
        UpdateActivePlayers(p =>
        {
            p.Operation = interaction.Killed.Contains(p.Player) ? Messages["killed"] : "";
            p.IsEnabled = !interaction.Except.Contains(p.Player);
            p.IsSelected = false;
        });

        UpdateActivePlayers(interaction);
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
        foreach (var activePlayer in ActivePlayers)
            if (predicate?.Invoke(activePlayer) ?? true)
                activePlayer.DoSilent(nameof(ActivePlayers), () => action(activePlayer));
    }

    private void OnActivePlayerChange(string? name = null)
    {
        if (interaction == null)
            return;

        UpdateActivePlayers(interaction);
    }
}
