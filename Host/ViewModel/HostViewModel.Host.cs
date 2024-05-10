using System.Data;
using Host.Model;
using Mafia.Extensions;
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

    public (User, string)[] GetUserRoles()
    {
        var gameRoles = ActiveRoles.Where(r => r.IsSelected).SelectMany(r => Enumerable.Range(0, r.Count).Select(_ => r.Role.Name)).ToArray();
        gameRoles.Shaffle(17, rnd);

        return gameRoles.Select((role, i) => (users[i], role)).ToArray();
    }

    private Color GetPlayerColor(Player p) => 
        options.GroupColors.FirstOrDefault(gc => gc.Group == p.Group.Name)?.Color ??
        options.GroupColors.FirstOrDefault(gc => gc.Group == p.TopGroup.Name)?.Color ?? 
        Colors.Black;

    public async Task StartGame(State state)
    {
        ActivePlayers = [];
        await Task.Delay(100); // skip list replace animations
        ActivePlayers = state.Players.Select(p => new ActivePlayer(p, OnActivePlayerChange, nameof(ActivePlayers))
        {
            TextColor = GetPlayerColor(p)
        }).OrderBy(p => p.Player.Group.Name).ThenBy(p => p.Player.Role.Rank).ToArray();
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
            await Interact(new Interaction
            {
                Name = "Killed",
                Args = [state.LatestNews.Killed.SJoin(", ")],
                Killed = state.LatestNews.Killed,
                State = state
            });

            ActivePlayers = ActivePlayers.Where(p => state.Players.Contains(p.Player)).ToArray();

            Log($"Alive players: {state.Players.SJoin(", ")}");
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
        if (state.DayNumber > 1)
        {
            await AskCityToWakeUp(state);

            Log($"===== </night {state.DayNumber}> =====");
        }

        Log($"===== <day {state.DayNumber}> =====");
    }

    public async Task NotifyNightStart(State state)
    {
        await AskCityToFallAsleep(state);

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

        ActivePlayers = ActivePlayers.Where(p => state.Players0.Contains(p.Player)).ToArray();

        await Interact(new Interaction
        {
            Name = "GameEnd",
            Args = [winnerGroup.Name],
            State = state
        });
    }

    public async Task<Player[]> AskCityToSelect(State state, CityAction action)
    {
        var result = await Interact(new Interaction
        {
            Name = action.IsSkippable() ? "CitySelectCanSkip" : "CitySelectNoSkip",
            Selection = (action.IsSkippable() ? 0 : 1, 1),
            State = state
        });

        return result.Selected;
    }

    public async Task<Player[]> GetNeighbors(State state, Player player, Action action)
    {
        var result = await Interact(new Interaction
        {
            Name = "RoundKilled",
            Args = [player.ToString()],
            Selection = (2, 2),
            State = state
        });

        return result.Selected;
    }

    public async Task<Player[]> AskToSelect(State state, Player player, Action action)
    {
        var result = await Interact(new Interaction
        {
            Name = action.IsSkippable() ? "PlayerSelectCanSkip" : "PlayerSelectNoSkip",
            Args = [player.Role.Name],
            Selection = (action.IsSkippable() ? 0 : 1, 1),
            Except = state.GetExceptPlayers(player),
            Player = player,
            State = state
        });

        return result.Selected;
    }

    private async Task AskCityToWakeUp(State state)
    {
        await Interact(new Interaction
        {
            Name = "WakeUpCity",
            State = state
        });
    }

    private async Task AskCityToFallAsleep(State state)
    {
        await Interact(new Interaction
        {
            Name = "FallAsleepCity",
            State = state
        });
    }
}
