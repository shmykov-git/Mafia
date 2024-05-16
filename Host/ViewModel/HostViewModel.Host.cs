using System.Data;
using System.Diagnostics;
using System.Linq;
using Host.Libraries;
using Host.Model;
using Mafia.Extensions;
using Mafia.Libraries;
using Mafia.Model;
using Action = Mafia.Model.Action;

namespace Host.ViewModel;

public partial class HostViewModel : IHost
{
    private int? seed = null;

    public void ChangeSeed(int seed)
    {
        this.seed = seed;
        rnd = new Random(seed);
    }

    public string[] GetGameRoles()
    {
        return GetSelectedMultipliedRoles().Select(r=>r.Name).ToArray();
    }

    private Color GetRoleColor(string roleName) => 
        GetGroupColor(city.GetGroupByRoleName(roleName)) ?? 
        GetGroupColor(city.GetTopGroupByRoleName(roleName)) ?? 
        options.CityColor;

    private Color? GetGroupColor(Group group) => options.GroupColors.FirstOrDefault(gc => gc.Group == group.Name)?.Color;
    private Color GetPlayerRoleColor(Player p) => GetGroupColor(p.Group) ?? GetGroupColor(p.TopGroup) ?? options.CityColor;

    private Color GetUserColor(Player p) => p.User == null 
        ? options.CityColor
        : GetPlayerRoleColor(p);

    public async Task StartGame(State state)
    {
        prevNightInteraction = null;
        ContinueMode = ContinueGameMode.RolesSelections;

        ActivePlayers = [];
        await Task.Delay(options.SkipAnimationDelay);
        ActivePlayers = ActiveUsers.Where(u => u.IsSelected).Select(u => new ActivePlayer(Messages, u.User, OnActivePlayerChange, nameof(ActivePlayers))
        {
            NickColor = options.CityColor,
            RoleColor = options.CityColor
        }).OrderBy(p => p.Nick).ToArray();
        //.OrderBy(p => p.Player.Group.Name).ThenBy(p => p.Player.Role.Rank).ToArray();

        replays.Add(state.Replay);        
    }

    public async Task NotifyGameEnd(State state, Group winnerGroup)
    {
        Log($"GameEnd, the winner is {winnerGroup.Name}");
        Log($"===== </day {state.DayNumber}> =====");

        await SaveGameReplay(state);

        ActivePlayerFilter.Killed = true;

        await Interact(new Interaction
        {
            Name = "GameEnd",
            Args = [winnerGroup.Name],
            State = state
        });

        if (navigationPath == Routes.GameView)
            await Shell.Current.GoToAsync(Routes.RolesView);
    }

    public async Task NotifyCityAfterNight(State state)
    {
        await TellTheNews(state);
    }

    public async Task NotifyCityAfterDay(State state)
    {
        await TellTheNews(state);
    }

    public async Task NotifyDayStart(State state)
    {
        if (state.DayNumber > 1)
        {
            //await AskCityToWakeUp(state);

            Log($"===== </night {state.DayNumber}> =====");
        }

        Log($"===== <day {state.DayNumber}> =====");
    }

    public async Task NotifyNightStart(State state)
    {
        await Interact(new Interaction
        {
            Name = "FallAsleepCity",
            State = state
        });

        Log($"===== </day {state.DayNumber}> =====");
        Log($"===== <night {state.DayNumber}> =====");
    }

    public async Task<bool> IsGameEnd(State state)
    {
        // Ведущий может остановить игру в результате математической победы (2 мафии, 2 мирных)
        return false;
    }

    public void RolledBack(State state) 
    {
        if (state.IsFirstDay)
        {
            var cityKilled = state.AllSelects().SelectMany(s => s.Whom).ToArray();
            ActivePlayers.Where(p => !cityKilled.Contains(p.Player)).Where(p => p.IsKnown).ForEach(DetachRole);
        }

        var factKills = state.AllFactKills();
        ActivePlayers.Where(a => !factKills.Contains(a.Player)).ForEach(p => p.IsAlive = true);

        Changed(nameof(FilteredActivePlayers));
    }

    public async Task Hello(State state, Player player) 
    {
        await Interact(new Interaction
        {
            Name = null!,
            Player = player,
            State = state
        });
    }

    private async Task TellTheNews(State state)
    {
        if (!state.HasNews)
        {
            await Interact(new Interaction
            {
                Name = "HelloCity",
                State = state
            });
        }
        else
        {
            var kills = state.LatestNews.FactKills.Select(p => ActivePlayers.Single(a => a.Player == p)).ToArray();

            if (state.IsMorning)
            {
                var newsName = state.LatestNews.FactKills.Any() ? "KillsInTheCity" : "NoKillsInTheCity";
                var (name, subName) = state.IsMorning ? ("WakeUpCity", newsName) : (newsName, "");
                List<HostTail> tails = new();

                if (state.DoesDoctorHaveThanks())
                    tails.Add(HostTail.ThanksToDoctor);

                if (state.DoesSomebodyExceptDoctorSkipKills())
                    tails.Add(HostTail.DoctorHasNoDeal);

                await Interact(new Interaction
                {
                    Name = name,
                    SubName = subName,
                    Args = [kills.Select(p => p.Nick).SJoin(", ")],
                    Killed = state.LatestNews.FactKills,
                    Tails = tails.ToArray(),
                    State = state
                });
            }

            state.LatestNews.FactKills.ForEach(p => ActivePlayers.First(a => a.Player == p).IsAlive = false);
            Changed(nameof(FilteredActivePlayers));

            Log($"Alive players: {state.Players.SJoin(", ")}");
        }
    }

    public async Task<User[]> AskCityToSelect(State state, CityAction action, string operation)
    {
        var result = await Interact(new Interaction
        {
            Name = action.IsSkippable() ? "CitySelectCanSkip" : "CitySelectNoSkip",
            Selection = (action.IsSkippable() ? 0 : 1, 1),
            Operation = operation,
            State = state
        });

        return result.SelectedUsers;
    }

    public async Task<User[]> GetNeighbors(State state, Player player, Action action, string operation)
    {
        // todo: exotic mafia?
        var result = await Interact(new Interaction
        {
            Name = action.IsSkippable() ? "RoundKilled" : "RoundKilled",
            Args = [player.ToString()],
            Selection = (action.IsSkippable() ? 0 : 2, 2),
            Except = state.GetExceptPlayers(player),
            Unwanted = Values.UnwantedOperations.Contains(operation) ? state.GetTeam(player) : [],
            Player = player,
            Operation = operation,
            State = state
        });

        return result.SelectedUsers;
    }

    public async Task<User[]> AskToSelect(State state, Player player, Action action, string operation)
    {
        (string role, string name, string nameOrSkip)[] data = 
        [
            (KnownRoles[KnownRoleKey.Doctor], "DoctorHealPlayer", "DoctorHealPlayerOrSkip"),
            (KnownRoles[KnownRoleKey.Commissar], "CommissarCheckPlayer", "CommissarCheckPlayerOrSkip"),
            (KnownRoles[KnownRoleKey.Maniac], "ManiacKillPlayer", "ManiacKillPlayerOrSkip"),
            ("kill", "PlayerKill", "PlayerKillOrSkip"),
            ("_", "PlayerSelect", "PlayerSelectOrSkip")
        ];

        var isKill = Values.KillOperations.Contains(operation);
        var line = data.First(v => v.role == player.Role.Name || (isKill ? v.role == "kill" : v.role == "_"));

        var result = await Interact(new Interaction
        {
            Name = action.IsSkippable() ? line.nameOrSkip : line.name,
            Args = [player.Group.Name],
            Selection = (action.IsSkippable() ? 0 : 1, 1),
            Except = state.GetExceptPlayers(player),
            Unwanted = Values.UnwantedOperations.Contains(operation) ? state.GetTeam(player) : [],
            Player = player,
            Operation = operation,
            State = state
        });

        return result.SelectedUsers;
    }
}
