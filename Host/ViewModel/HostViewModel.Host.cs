using System.Data;
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
        prevInteraction = null;
        ContinueMode = ContinueGameMode.Interaction;

        ActivePlayers = [];
        await Task.Delay(options.SkipAnimationDelay);
        ActivePlayers = ActiveUsers.Where(u => u.IsSelected).Select(u => new ActivePlayer(Messages, u.User, OnActivePlayerChange, nameof(ActivePlayers))
        {
            NickColor = options.CityColor,
            RoleColor = options.CityColor
        }).OrderBy(p => p.Nick).ToArray();
            //.OrderBy(p => p.Player.Group.Name).ThenBy(p => p.Player.Role.Rank).ToArray();
    }

    public async Task NotifyCityAfterNight(State state)
    {
        await TellTheNews(state, true);
    }

    public async Task NotifyCityAfterDay(State state)
    {
        await TellTheNews(state, false);
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

    public async Task NotifyGameEnd(State state, Group winnerGroup)
    {
        Log($"GameEnd, the winner is {winnerGroup.Name}");
        Log($"===== </day {state.DayNumber}> =====");

        //ActivePlayers = ActivePlayers.Where(p => state.Players0.Contains(p.Player)).ToArray();

        await Interact(new Interaction
        {
            Name = "GameEnd",
            Args = [winnerGroup.Name],
            State = state
        });
    }

    private async Task TellTheNews(State state, bool afterNight)
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
            var kills = state.LatestNews.FactKilled.Select(p => ActivePlayers.Single(a => a.Player == p)).ToArray();

            if (state.IsNight || kills.Length > 0)
            {
                var newsName = state.LatestNews.FactKilled.Any() ? "KillsInTheCity" : "NoKillsInTheCity";
                var (name, subName) = afterNight ? ("WakeUpCity", newsName) : (newsName, "");

                await Interact(new Interaction
                {
                    Name = name,
                    SubName = subName,
                    Args = [kills.Select(p => p.Nick).SJoin(", ")],
                    Killed = state.LatestNews.FactKilled,
                    State = state
                });
            }

            state.LatestNews.FactKilled.ForEach(p => ActivePlayers.First(a => a.Player == p).IsAlive = false);
            Changed(nameof(FilteredActivePlayers));
            //ActivePlayers = ActivePlayers.Where(p => p.Player == null || state.Players.Contains(p.Player)).ToArray();

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
        var result = await Interact(new Interaction
        {
            Name = action.IsSkippable() ? "PlayerSelectCanSkip" : "PlayerSelectNoSkip",
            Args = [player.Role.Name],
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
