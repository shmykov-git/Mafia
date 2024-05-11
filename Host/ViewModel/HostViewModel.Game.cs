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
    private string _selectedPlayerRoleMessage;
    private string _playerInfo;
    private Color _hintColor;
    private Color _selectedPlayerRoleMessageColor;
    private ActiveRole[] _activePlayerRoles;
    private ActivePlayer[] _activePlayers;
    private Interaction? prevInteraction = null;
    private Interaction? interaction = null;
    private string activePlayerSelectionMode = "interaction";

    public Color HintColor { get => _hintColor; set { _hintColor = value; Changed(); } }
    public bool IsVisibleHostWakeUpHint { get => _isVisibleHostWakeUpHint; set { _isVisibleHostWakeUpHint = value; Changed(); } }
    public string HostWakeUpHint { get => _hostWakeUpHint; set { _hostWakeUpHint = value; Changed(); } }
    public string HostHint { get => _hostHint; set { _hostHint = value; Changed(); } }
    public string PlayerInfo { get => _playerInfo; set { _playerInfo = value; Changed(); } }
    public ActivePlayer[] ActivePlayers { get => _activePlayers; set { _activePlayers = value; ChangedSilently(); } }
    public ActiveRole[] ActivePlayerRoles { get => _activePlayerRoles; set { _activePlayerRoles = value; ChangedSilently(); } }
    public bool IsActivePlayerRoleVisible { get => _isActivePlayerRoleVisible; set { _isActivePlayerRoleVisible = value; Changed(); Changed(nameof(ContinueCommand)); } }
    public string SelectedPlayerRoleMessage { get => _selectedPlayerRoleMessage; set { _selectedPlayerRoleMessage = value; Changed(); } }
    public Color SelectedPlayerRoleMessageColor { get => _selectedPlayerRoleMessageColor; set { _selectedPlayerRoleMessageColor = value; Changed(); } }


    public ICommand ContinueCommand => new Command(()=>Continue(), 
        () => game.IsActive && !IsActivePlayerRoleVisible && (interaction == null || ActivePlayers.Count(p => p.IsSelected).Between(interaction.Selection)));

    private void ShowWakeUpMessage()
    {
        if (interaction.State.IsDay || interaction.Player == null)
            return;
                
        HintColor = GetRoleColor(interaction.Player.Role.Name);

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

    void AttachRole(Interaction interaction, ActivePlayer activePlayer, Role role)
    {
        var player = interaction.State.Players0.First(p => p.Role == role);
        activePlayer.Player = player;
        player.User = activePlayer.User;
        activePlayer.RoleColor = activePlayer.NickColor = GetRoleColor(activePlayer.RoleName);
    }

    private async Task AttachPlayerRoles(Interaction interaction, ActivePlayer[] activePlayers, Role[] roles)
    {
        var roleList = roles.ToList();

        foreach (var activePlayer in activePlayers)
        {
            if (roleList.Distinct().Count() == 1)
            {
                AttachRole(interaction, activePlayer, roleList[0]);
                roleList.RemoveAt(0);
            }
            else
            {
                SelectedPlayerRoleMessage = $"Выберите роль игрока {activePlayer.Nick}";
                await PrepareActivePlayerRoles(interaction, roleList.Distinct().ToArray());
                IsActivePlayerRoleVisible = true;
                await WaitForHostInteraction();
                IsActivePlayerRoleVisible = false;
                var selectedRole = ActivePlayerRoles.Single(r => r.IsSelected);                
                AttachRole(interaction, activePlayer, selectedRole.Role);
                selectedRole.RoleColor = activePlayer.RoleColor;
                roleList.Remove(selectedRole.Role);
            }
        }
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

        if (interaction.State.DayNumber == 1 && interaction.Player != null)
        {
            activePlayerSelectionMode = "firstWakeup";
            PrepareActivePlayersForFirstWakeup();

            var groupRoles = interaction.Player.Group.AllRoles().ToArray();
            var wakeupRoles = GetSelectedMultipliedRoles().Where(groupRoles.Contains).ToList();
            var killedRoles = interaction.State.LatestDayNews.Killed.Select(p => p.Role).Where(wakeupRoles.Contains).ToArray();
            killedRoles.ForEach(r => wakeupRoles.Remove(r));

            firstWakeupCount = wakeupRoles.Count;

            await WaitForHostInteraction();
            activePlayerSelectionMode = "interaction";

            var wakeUpPlayers = ActivePlayers.Where(p => p.IsSelected).ToArray();
            await AttachPlayerRoles(interaction, wakeUpPlayers, wakeupRoles.ToArray());

            var lastRoles = GetSelectedRoles().Except(ActivePlayers.Where(p => p.Player != null).Select(p => p.Player.Role)).ToArray();
            
            if (lastRoles.Length == 1)
            {
                ActivePlayers.Where(p => p.Player != null).ForEach(p => AttachRole(interaction, p, lastRoles[0]));
            }
        }

        ShowHostMessage();
        PrepareActivePlayers(interaction);

        await WaitForHostInteraction();

        var result = new InteractionResult() 
        {
            Selected = ActivePlayers.Where(p => p.IsSelected).ToArray()
        };
        
        // todo: rule show card
        if (interaction.State.IsDay)
            await AttachPlayerRoles(interaction, result.Selected, GetSelectedRoles());

        CleanUpInteraction();

        if (result.IsSkipped)
            Log($"{(interaction.WithCity ? "City" : interaction.Player)}, skip selection");
        else
            Log($"{(interaction.WithCity ? "City" : interaction.Player)} --> {result.Selected.SJoin(", ")}");

        prevInteraction = interaction;
        this.interaction = null;

        return result;
    }

    private void Continue(int timeout = 20)
    {
        Task.Run(async () =>
        {
            await Task.Delay(timeout);
            hostWaiter?.SetResult();
        });
    }

    private async Task PrepareActivePlayerRoles(Interaction interaction, Role[] roles)
    {
        ActivePlayerRoles = roles.Select(r => new ActiveRole(r, OnActivePlayerRoleChange, nameof(ActivePlayerRoles)) { RoleColorSilent = GetRoleColor(r.Name), IsEnabledSilent = true }).ToArray();
    }

    object skipKey = null;
    private void OnActivePlayerRoleChange(string name, ActiveRole role)
    {
        var isSelected = ActivePlayerRoles.Count(a => a.IsSelected) == 1;

        if (isSelected && ActivePlayerRoles != skipKey)
        {
            skipKey = ActivePlayerRoles;
            Continue();
        }
    }

    private void UpdateActivePlayerRoles(Action<ActiveRole> action, Func<ActiveRole, bool>? predicate = null)
    {
        foreach (var activeRole in ActivePlayerRoles)
            if (predicate?.Invoke(activeRole) ?? true)
                activeRole.DoSilent(nameof(ActivePlayerRoles), () => action(activeRole));
    }

    private string arrow = "⮕";
    private string minus = "⭐";
    private int firstWakeupCount = 0;

    private void PrepareActivePlayersForFirstWakeup()
    {
        UpdateActivePlayers(p =>
        {
            p.Operation = arrow;
            p.OperationColor = options.CityColor;
            p.IsEnabled = true;
            p.IsSelected = false;
        });
    }

    private void FirstWakeupChanged()
    {
        if (ActivePlayers.Where(p => p.IsSelected).Count() == firstWakeupCount)
            Continue();
    }

    private void PrepareActivePlayers(Interaction interaction)
    {
        var operationColor = interaction.NeedSelection ? Colors.Red : options.CityColor;
        var operation = interaction.NeedSelection ? arrow : minus;

        UpdateActivePlayers(p =>
        {
            p.Operation = interaction.Killed.Contains(p.Player) ? Messages["killed"] : operation;
            p.OperationColor = operationColor;
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

        switch (activePlayerSelectionMode)
        {
            case "interaction":
                UpdateActivePlayers(interaction);
                break;
            case "firstWakeup":
                FirstWakeupChanged();
                break;
        }
        
    }
}
