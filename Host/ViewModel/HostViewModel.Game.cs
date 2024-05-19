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
    private Interaction? prevNightInteraction = null;

    private string _hostHint;
    private string _gameInfo;
    private string _hostWakeUpHint;
    private bool _isActivePlayerRoleVisible;
    private bool _isRollbackAvailable;
    private string _selectedPlayerRoleMessage;
    private string _playerInfo;
    private Color _hintColor;
    private Color _selectedPlayerRoleMessageColor;
    private ActiveRole[] _activePlayerRoles;
    private ActivePlayer[] _activePlayers = [];
    private ContinueGameMode _continueMode = ContinueGameMode.RolesSelections;
    private ActivePlayerFilter _activePlayerFilter;
    private Interaction? _interaction = null;

    private string arrow = "⮕";
    private string empty = "";
    private string killed = "💀";
    private int wakeupRolesCount = 0;

    public bool IsGameOn => hostWaiter != null;
    public bool IsGameTabAvailable => ActivePlayers.Length > 0;

    private Interaction Interaction { get => _interaction; set { _interaction = value; Changed(nameof(ContinueCommand)); } }
    private ContinueGameMode ContinueMode { get => _continueMode; set { _continueMode = value; Changed(nameof(ContinueCommand)); } }

    public ActivePlayerFilter ActivePlayerFilter => _activePlayerFilter ??= new ActivePlayerFilter(()=>Changed(nameof(FilteredActivePlayers))) { Killed = false };
    public Color HintColor { get => _hintColor; set { _hintColor = value; Changed(); } }
    public bool IsSubHostHintVisible => SubHostHint.HasText();
    public string SubHostHint { get => _hostWakeUpHint; set { _hostWakeUpHint = value; Changed(); Changed(nameof(IsSubHostHintVisible)); } }
    public bool IsHostHintVisible => HostHint.HasText();
    public string HostHint { get => _hostHint; set { _hostHint = value; Changed(); Changed(nameof(IsHostHintVisible)); } }

    public ActivePlayer[] ActivePlayers { get => _activePlayers; set { _activePlayers = value; ChangedSilently(); Changed(nameof(FilteredActivePlayers)); Changed(nameof(ContinueCommand)); Changed(nameof(IsGameTabAvailable)); } }
    public IEnumerable<ActivePlayer> FilteredActivePlayers => ActivePlayers.Where(p => ActivePlayerFilter.Killed || p.IsAlive);
    public IEnumerable<ActivePlayer> AliveActivePlayers => ActivePlayers.Where(p => p.IsAlive);
    public ActiveRole[] ActivePlayerRoles { get => _activePlayerRoles; set { _activePlayerRoles = value; ChangedSilently(); Changed(nameof(ContinueCommand)); } }
    public bool IsActivePlayerRoleVisible { get => _isActivePlayerRoleVisible; set { _isActivePlayerRoleVisible = value; Changed(); Changed(nameof(ContinueCommand)); } }
    public bool IsRollbackAvailable { get => _isRollbackAvailable; set { _isRollbackAvailable = value; Changed(); Changed(nameof(RollbackCommand), nameof(IsRollbackNotAvailable)); } }
    public bool IsRollbackNotAvailable { get => !IsRollbackAvailable; set { IsRollbackAvailable = !value; Changed(); Changed(nameof(RollbackCommand), nameof(IsRollbackAvailable)); } }
    public string SelectedPlayerRoleMessage { get => _selectedPlayerRoleMessage; set { _selectedPlayerRoleMessage = value; Changed(); } }
    public Color SelectedPlayerRoleMessageColor { get => _selectedPlayerRoleMessageColor; set { _selectedPlayerRoleMessageColor = value; Changed(); } }

    public string GameInfo => Interaction == null ? "" : 
        Messages["GameInfo"].With(
            Interaction.State.DayNumber,
            Interaction.State.IsNight ? Messages["night"] : (Interaction.State.IsMorning ? Messages["morning"] : Messages["evening"]),
            Interaction.State.Players.Count,
            Interaction.State.Players0.Length);

    private bool IsContinueAvailable()
    {
        if (!game.IsActive || Interaction == null)
            return false;

        return ContinueMode switch
        {
            ContinueGameMode.RolesSelections => ActivePlayers.Count(p => p.IsSelected).Between(Interaction.Selection) || ActivePlayers.All(p => p.IsSelected || p.IsDisabled),
            ContinueGameMode.WakeupOnFirstDay => ActivePlayers.Count(p => p.IsSelected) == Interaction.WakeupRoles.Length,
            ContinueGameMode.PreviousGroupFallAsleep => false,
            _ => false,
        };
    }

    public ICommand ContinueCommand => new Command(() => Continue(), IsContinueAvailable);
    public ICommand RollbackCommand => new Command(() => Rollback(), () => IsRollbackAvailable);

    private async Task<InteractionResult> Interact(Interaction interaction)
    {
        Interaction = interaction;

        if (Interaction.State.Stopping || Interaction.State.RollingBack)
        {
            prevNightInteraction = null;
            return new InteractionResult();
        }

        interaction.WakeupRoles = GetWakeupRoles();

        Changed(nameof(GameInfo));

        await ShowFallAsleepMessage();
        ShowWakeUpMessage();

        if (interaction.NeedFirstDayWakeup)
            await FirstDayWakeup();

        await ShowHostMainMessage();

        var result = new InteractionResult()
        {
            Selected = ActivePlayers.Where(p => p.IsSelected).ToArray()
        };

        // todo: rule show card
        if (interaction.State.IsDay && interaction.State.DayNumber == 1 && !interaction.SkipRoleSelection)
            await AttachPlayerRoles(Interaction.State, result.Selected, GetSelectedRoles());

        CleanUpInteraction();

        prevNightInteraction = interaction;
        Interaction = null;

        return result;
    }

    private Role[] GetWakeupRoles()
    {
        if (Interaction.Player == null)
            return [];

        var groupRoles = Interaction.Player.Group.AllRoles().ToArray();
        var wakeupRoles = GetSelectedMultipliedRoles().Where(groupRoles.Contains).ToList();
        var killedRoles = ActivePlayers.Where(p=>!p.IsAlive).Select(p=>p.Player.Role).ToArray();
        killedRoles.ForEach(r => wakeupRoles.Remove(r));
        return wakeupRoles.ToArray();
    }

    private Role[] GetDetachedPlayerGroupRoles()
    {
        if (Interaction?.Player == null)
            return [];

        var playerGroupRoles = Interaction.Player.Group.AllRoles().ToArray();
        var detachedRoles = GetSelectedMultipliedRoles().Where(playerGroupRoles.Contains).ToList();
        var attachedRoles = ActivePlayers.Where(p => p.Player != null).Select(p => p.Player.Role).ToArray();
        attachedRoles.ForEach(r => detachedRoles.Remove(r));
        return detachedRoles.ToArray();
    }

    private Role[] GetDetachedUniqueRoles()
    {
        if (Interaction?.Player == null)
            return [];

        var detachedRoles = GetSelectedMultipliedRoles().ToList();
        var attachedRoles = ActivePlayers.Where(p => p.Player != null).Select(p => p.Player.Role).ToArray();
        attachedRoles.ForEach(r => detachedRoles.Remove(r));
        return detachedRoles.Distinct().ToArray();
    }

    private async Task FirstDayWakeup()
    {
        if (Interaction.State.Stopping || Interaction.State.RollingBack)
            return;

        var detachedGroupRoles = GetDetachedPlayerGroupRoles();

        if (detachedGroupRoles.Length == 0) 
            return;

        SubHostHint = detachedGroupRoles.Length == 1
            ? Messages["SelectWokeupPlayer"]
            : Messages["SelectAllWokeupPlayers"].With(detachedGroupRoles.Length);

        ContinueMode = ContinueGameMode.WakeupOnFirstDay;
        
        PrepareActivePlayers_WakeupOnFirstDay();

        wakeupRolesCount = detachedGroupRoles.Length;

        await WaitForHostInteraction();

        ContinueMode = ContinueGameMode.RolesSelections;

        var selectedPlayers = ActivePlayers.Where(p => p.IsSelected).ToArray();
        await AttachPlayerRoles(Interaction.State, selectedPlayers, detachedGroupRoles.ToArray());

        var lastUniqueRoles = GetDetachedUniqueRoles();

        if (lastUniqueRoles.Length == 1)
        {
            ActivePlayers.Where(p => p.Player == null).ToArray().ForEach(p => AttachRole(p, lastUniqueRoles[0]));
        }
    }

    private void Continue(int timeout = 20)
    {
        Task.Run(async () =>
        {
            await Task.Delay(timeout);
            hostWaiter?.SetResult();
        });
    }

    private void Rollback(int timeout = 20)
    {
        Interaction.State.Rollback();
        Continue(timeout);
    }

    private void ShowWakeUpMessage()
    {
        if (Interaction.State.Stopping || Interaction.State.RollingBack)
            return;

        if (Interaction.State.IsDay || Interaction.Player == null)
            return;
        
        var count = Interaction.WakeupRoles.Length;

        var wakeUpMessage = count == 1
            ? Messages["PlayerWakeUp"].With(Interaction.Player.Role.Name)
            : Messages["GroupWakeUp"].With(Interaction.Player.Group.Name, count, Interaction.State.GetGroupActivePlayers(Interaction.Player.Group).Select(p => p.Role.Name).SJoin(", "));

        HintColor = GetRoleColor(Interaction.Player.Role.Name);
        HostHint = wakeUpMessage;
        SubHostHint = "";
    }

    private async Task ShowFallAsleepMessage()
    {
        if (Interaction.State.Stopping || Interaction.State.RollingBack) 
            return;

        if (prevNightInteraction?.Player?.Group == null || prevNightInteraction.Player.Group == Interaction.Player?.Group)
            return;
        
        var fallAsleepMessage = prevNightInteraction.WakeupRoles.Length == 1
            ? Messages["PlayerFallAsleep"].With(prevNightInteraction.Player.Role.Name)
            : Messages["GroupFallAsleep"].With(prevNightInteraction.Player.Group.Name);

        ContinueMode = ContinueGameMode.PreviousGroupFallAsleep;        
        HintColor = options.Theme.FallAsleepColor;
        HostHint = fallAsleepMessage;
        SubHostHint = "";
        PrepareActivePlayers_PreviousGroupFallAsleep();
        await Task.Delay(options.HostFallAsleepMessageDelay);
        HostHint = "";
        ContinueMode = ContinueGameMode.RolesSelections;
    }

    private async Task ShowHostMainMessage()
    {
        if (Interaction.State.Stopping || Interaction.State.RollingBack)
            return;

        if (!Interaction.Name.HasText())
            return;

        var message = Messages[Interaction.Name].With(Interaction.Args);
        var tailMessage = Interaction.Tails.Select(t => t switch
        {
            HostTail.ThanksToDoctor => Messages["ThanksToDoctor"],
            HostTail.DoctorHasNoDeal => Messages["DoctorHasNoDeal"],
        }).SJoin(" ");

        HintColor = Interaction.Player != null
            ? GetRoleColor(Interaction.Player.Role.Name)
            : options.Theme.CityColor;

        if (Interaction.SubName.HasText())
        {
            var subMessage = Messages[Interaction.SubName].With(Interaction.Args);
            HostHint = message;
            SubHostHint = tailMessage.HasText() ? $"{subMessage}. {tailMessage}" : subMessage;
        }
        else
        {
            SubHostHint = tailMessage.HasText() ? $"{message}. {tailMessage}" : message;
        }

        PrepareActivePlayers_RolesSelections();
        await WaitForHostInteraction();
    }

    private async Task WaitForHostInteraction()
    {
        if (Interaction.State.Stopping || Interaction.State.RollingBack)
            return;

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

    void AttachRole(ActivePlayer activePlayer, Role role)
    {
        var player = Interaction.State.Players0.Where(p => p.User == null).First(p => p.Role == role);
        activePlayer.Player = player;
        player.User = activePlayer.User;
        activePlayer.RoleColor = activePlayer.NickColor = GetRoleColor(activePlayer.RoleName);
    }

    void DetachRole(ActivePlayer activePlayer)
    {
        activePlayer.Player.User = null;
        activePlayer.Player = null;
        activePlayer.RoleColor = activePlayer.NickColor = options.Theme.CityColor;
    }

    private async Task AttachPlayerRoles(State state, ActivePlayer[] activePlayers, Role[] roles)
    {
        var roleList = roles.ToList();

        foreach (var activePlayer in activePlayers)
        {
            if (roleList.Distinct().Count() == 1)
            {
                AttachRole(activePlayer, roleList[0]);
                roleList.RemoveAt(0);
            }
            else
            {
                SelectedPlayerRoleMessage = Messages["WhatIsThePlayerRole"].With(activePlayer.Nick);
                await PrepareActivePlayerRoles(roleList.Distinct().ToArray());
                IsActivePlayerRoleVisible = true;
                await WaitForHostInteraction();
                IsActivePlayerRoleVisible = false;

                if (state.Stopping || state.RollingBack)
                    return;

                var selectedRole = ActivePlayerRoles.Single(r => r.IsSelected);                
                AttachRole(activePlayer, selectedRole.Role);
                selectedRole.RoleColor = activePlayer.RoleColor;
                roleList.Remove(selectedRole.Role);
            }
        }
    }

    private void CleanUpInteraction()
    {
        HostHint = "";
        SubHostHint = "";
        HintColor = options.Theme.CityColor;
    }

    private async Task PrepareActivePlayerRoles(Role[] roles)
    {
        ActivePlayerRoles = roles.Select(r => new ActiveRole(r, OnActivePlayerRoleChange, nameof(ActivePlayerRoles)) { RoleColorSilent = GetRoleColor(r.Name) }).ToArray();
    }

    object skipRolesKey = null;
    private void OnActivePlayerRoleChange(string name, ActiveRole role)
    {
        var isSelected = ActivePlayerRoles.Count(a => a.IsSelected) == 1;

        if (isSelected && ActivePlayerRoles != skipRolesKey)
        {
            skipRolesKey = ActivePlayerRoles;
            Continue();
        }
    }

    private void UpdateActivePlayerRoles(Action<ActiveRole> action, Func<ActiveRole, bool>? predicate = null)
    {
        foreach (var activeRole in ActivePlayerRoles)
            if (predicate?.Invoke(activeRole) ?? true)
                activeRole.DoSilent(nameof(ActivePlayerRoles), () => action(activeRole));
    }

    private void PrepareActivePlayers_PreviousGroupFallAsleep()
    {
        UpdateActivePlayers(p =>
        {
            p.Operation = p.IsKilled ? killed : arrow;
            p.OperationColor = p.IsKilled ? options.Theme.KilledColor : options.Theme.NoOperationColor;
            p.CheckboxColor = options.Theme.NoOperationColor;
            p.IsEnabled = false;
            p.IsSelected = false;
        });

        Changed(nameof(ContinueCommand));
    }

    private void PrepareActivePlayers_WakeupOnFirstDay()
    {
        UpdateActivePlayers(p =>
        {
            var isEnabled = p.IsUnknown && p.IsAlive;

            p.Operation = p.IsKilled ? killed : arrow;
            p.OperationColor = p.IsKilled ? options.Theme.KilledColor : (isEnabled ? options.Theme.WakeupColor : options.Theme.NoOperationColor);
            p.CheckboxColor = isEnabled ? options.Theme.WakeupColor : options.Theme.NoOperationColor;
            p.IsEnabled = isEnabled;
            p.IsSelected = false;
        });

        Changed(nameof(ContinueCommand));
    }

    private void UpdateActivePlayer_WakeupOnFirstDay()
    {
        var areSelected = ActivePlayers.Where(p => p.IsSelected).Count() == wakeupRolesCount;

        UpdateActivePlayers(p =>
        {
            p.IsEnabled = p.IsUnknown && !areSelected;
        }, p=> p.IsAlive && !p.IsSelected);
    }

    private void PrepareActivePlayers_RolesSelections()
    {
        var staticOperationColor = Interaction.NeedSelection ? GetOperationColor(Interaction.Operation) : options.Theme.NoOperationColor;
        var color = Interaction.NeedSelection ? GetOperationColor(Interaction.Operation) : options.Theme.CityColor;
        var staticOperation = Interaction.NeedSelection ? arrow : empty;

        UpdateActivePlayers(p =>
        {
            var isKilled = Interaction.Killed.Contains(p.Player) || !p.IsAlive;
            var isEnabled = !Interaction.Except.Contains(p.User) && !isKilled;
            var operation = isKilled ? killed : staticOperation;
            var color = Interaction.Unwanted.Contains(p.Player) ? options.Theme.UnwantedColor : staticOperationColor;

            p.Operation = operation;
            p.OperationColor = isKilled ? options.Theme.KilledColor : (isEnabled ? color : options.Theme.NoOperationColor);
            p.CheckboxColor = isEnabled ? color : options.Theme.NoOperationColor;
            p.IsEnabled = isEnabled;
            p.IsSelected = false;
        });

        UpdateActivePlayers_RolesSelections(Interaction);

        Changed(nameof(ContinueCommand));
    }

    private void UpdateActivePlayers_RolesSelections(Interaction interaction)
    {
        var staticOperationColor = Interaction.NeedSelection ? GetOperationColor(Interaction.Operation) : options.Theme.NoOperationColor;
        var color = Interaction.NeedSelection ? GetOperationColor(Interaction.Operation) : options.Theme.CityColor;
        var staticIsEnabled = ActivePlayers.Count(a => a.IsSelected).Between(0, interaction.Selection.to - 1);

        UpdateActivePlayers(p =>
        {
            var isKilled = !p.IsAlive;
            var isEnabled = staticIsEnabled && !interaction.Except.Contains(p.User);
            var color = Interaction.Unwanted.Contains(p.Player) ? options.Theme.UnwantedColor : staticOperationColor;

            p.OperationColor = isKilled ? options.Theme.KilledColor : (isEnabled ? color : options.Theme.NoOperationColor);
            p.CheckboxColor = isEnabled ? color : options.Theme.NoOperationColor;
            p.IsEnabled = isEnabled;
        }, p => p.IsAlive && !p.IsSelected);
    }

    private void UpdateActivePlayers(Action<ActivePlayer> action, Func<ActivePlayer, bool>? predicate = null)
    {
        foreach (var activePlayer in ActivePlayers)
            if (predicate?.Invoke(activePlayer) ?? true)
                activePlayer.DoSilent(nameof(ActivePlayers), () => action(activePlayer));
    }

    private void OnActivePlayerChange(string? name = null)
    {
        if (Interaction == null)
            return;

        switch (ContinueMode)
        {
            case ContinueGameMode.RolesSelections:
                UpdateActivePlayers_RolesSelections(Interaction);
                break;
            case ContinueGameMode.WakeupOnFirstDay:
                UpdateActivePlayer_WakeupOnFirstDay();
                break;
        }

        Changed(nameof(ContinueCommand));
    }
}
