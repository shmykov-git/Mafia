using System.Data;
using System.Diagnostics;
using System.Linq;
using Host.Libraries;
using Host.Model;
using Mafia.Executions;
using Mafia.Extensions;
using Mafia.Libraries;
using Mafia.Model;
using Action = Mafia.Model.Action;

namespace Host.ViewModel;

public partial class HostViewModel : IHost
{
    public void ChangeSeed(int seed)
    {
    }

    public User[] GetGameUsers() => ActiveUsers.Where(u => u.IsSelected).Select(u => u.User).ToArray();
    public string[] GetGameRoles()
    {
        return GetSelectedMultipliedRoles().Select(r=>r.Name).ToArray();
    }

    private Color GetRoleColor(string roleName) => GetGroupColor(city.GetGroupsByRoleName(roleName).FirstOrDefault()) ?? options.Theme.CityColor;

    private Color? GetGroupColor(Group? group) => group == null ? null : language.GroupColors.FirstOrDefault(gc => gc.Group == group.Name)?.Color;
    private Color GetPlayerRoleColor(Player p) => GetGroupColor(p.Group) ?? GetGroupColor(p.TopGroup) ?? options.Theme.CityColor;

    private Color GetUserColor(Player p) => p.User == null 
        ? options.Theme.CityColor
        : GetPlayerRoleColor(p);

    public async Task StartGame(State state)
    {
        prevNightInteraction = null;
        ContinueMode = ContinueGameMode.RolesSelections;

        ActivePlayers = [];
        await Task.Delay(options.SkipAnimationDelay);
        ActivePlayers = ActiveUsers.Where(u => u.IsSelected).Select(u => new ActivePlayer(Messages, u.User, OnActivePlayerChange, nameof(ActivePlayers))
        {
            NickColor = options.Theme.CityColor,
            RoleColor = options.Theme.CityColor
        }).OrderBy(p => p.Nick).ToArray();
        //.OrderBy(p => p.Player.Group.Name).ThenBy(p => p.Player.Role.Rank).ToArray();

        replays.Add(state.Replay);        
    }

    private async Task ApplyGameReplay(State state)
    {
        if (!state.AreAllKnown)
            return;

        var players = state.Players0.ToList();
        int[] GetWhom(Select s) => s.Whom.Select(p => players.IndexOf(p)).ToArray();
        int GetWho(Select s) => s.IsCity ? -1 : players.IndexOf(s.Who);

        state.Replay.Players = state.Players0.Select(p => (p.User.Nick, p.Role.Name)).ToArray();
        state.Replay.Selections = state.News.Select(n => n.Selects.Select(s => (GetWho(s), GetWhom(s))).ToArray()).ToArray();
    }

    public async Task NotifyGameEnd(State state, Group winnerGroup)
    {
        await ApplyGameReplay(state);
        await SaveGameReplay(state);

        var rating = await referee.GetRating(state.Replay, state.City);

        if (rating.IsSupported)
        {
            await ApplyRatings(state, rating);
        }

        ActivePlayerFilter.Killed = true;

        await Interact(new Interaction
        {
            Name = "GameEnd",
            Args = [winnerGroup.Name],
            State = state
        });

        if (rating.IsSupported)
        {
            if (navigationPath == HostValues.GameView)
                await Shell.Current.GoToAsync(HostValues.RatingView);
        }
        else
        {
            if (navigationPath == HostValues.GameView)
                await Shell.Current.GoToAsync(HostValues.RolesView);
        }
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
    }

    public async Task NotifyNightStart(State state)
    {
        await Interact(new Interaction
        {
            Name = "FallAsleepCity",
            State = state
        });
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

                if (state.IsAnybodyBanned())
                    tails.Add(HostTail.CityBansPlayer);

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
        }
    }

    public async Task<User[]> AskCityToSelect(State state, CityAction action, string operation)
    {
        if (operation == nameof(CityOperations.CityBan))
        {
            if (!IsBanAvailable || state.IsFirstDay)
                return [];

            var result = await Interact(new Interaction
            {
                Name = "CityBan",
                Selection = (0, 5), // todo: options
                Operation = operation,
                State = state
            });

            return result.SelectedUsers;
        }

        if (operation == nameof(CityOperations.CityKill))
        {
            var result = await Interact(new Interaction
            {
                Name = action.IsSkippable() ? "CitySelectCanSkip" : "CitySelectNoSkip",
                Selection = (action.IsSkippable() ? 0 : 1, 1),
                GetExceptFn = () => state.GetCityExceptUsers(operation),
                Operation = operation,
                State = state
            });

            return result.SelectedUsers;
        }

        if (operation == nameof(CityOperations.CityImmunity))
        {
            var result = await Interact(new Interaction
            {
                Name = "CityImmunity",
                SkipRoleSelection = true,
                Selection = (0, 5), // todo: options
                Operation = operation,
                State = state
            });

            return result.SelectedUsers;
        }

        throw new NotImplementedException(operation);
    }

    public async Task<User[]> GetNeighbors(State state, Player player, Action action, string operation)
    {
        // todo: exotic mafia?
        var result = await Interact(new Interaction
        {
            Name = action.IsSkippable() ? "RoundKilled" : "RoundKilled",
            Args = [player.ToString()],
            Selection = (action.IsSkippable() ? 0 : 2, 2),
            GetExceptFn = () => state.GetExceptUsers(player, operation, action.Arguments),
            GetUnwantedFn = () => state.GetUnwantedUsers(player, operation, action.Arguments),
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
            (KnownRoles[KnownRoleKey.Putana], "PutanaVisitPlayer", "PutanaVisitPlayerOrSkip"),
            (KnownRoles[KnownRoleKey.Prostitute], "ProstituteVisitPlayer", "ProstituteVisitPlayerOrSkip"),
            ("kill", "PlayerKill", "PlayerKillOrSkip"),
            ("check", "PlayerCheck", "PlayerCheckOrSkip"),
            ("_", "PlayerSelect", "PlayerSelectOrSkip")
        ];

        var isCheck = Values.CheckOperations.Contains(operation);
        var isKill = Values.KillOperations.Contains(operation);
        var dataRole = isKill ? "kill" : (isCheck ? "check" : "_");

        var line = data.First(v => v.role == player.Role.Name || v.role == dataRole);

        var result = await Interact(new Interaction
        {
            Name = action.IsSkippable() ? line.nameOrSkip : line.name,
            Args = [action.ByGroup ? player.Group.Name : player.Role.Name],
            Selection = (action.IsSkippable() ? 0 : 1, 1),
            GetExceptFn = () => state.GetExceptUsers(player, operation, action.Arguments),
            GetUnwantedFn = () => state.GetUnwantedUsers(player, operation, action.Arguments),
            Player = player,
            Operation = operation,
            State = state
        });

        return result.SelectedUsers;
    }
}
