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
        var nMax = 20; // пользователи в базе данных
        var usersDataBase = Enumerable.Range(1, nMax + 1).Select(i => new User { Nick = $"Nick{i}" }).ToArray();
        var userList = usersDataBase.ToList();

        var n = 15; // пришло поиграть
        var users = Enumerable.Range(1, n + 1).Select(_ =>
        {
            var i = rnd.Next(userList.Count);
            var player = userList[i];
            userList.RemoveAt(i);
            return player;
        }).ToArray();

        var gameRoles = ActiveRoles.Where(r => r.IsSelected).SelectMany(r => Enumerable.Range(0, r.Count).Select(_ => r.Role.Name)).ToArray();
        gameRoles.Shaffle(17, rnd);

        return gameRoles.Select((role, i) => (users[i], role)).ToArray();
    }

    private Color GetPlayerColor(Player p) => 
        options.GroupColors.FirstOrDefault(gc => gc.Group == p.Group.Name)?.Color ??
        options.GroupColors.FirstOrDefault(gc => gc.Group == p.TopGroup.Name)?.Color ?? 
        Colors.Black;

    public void StartGame(State state)
    {
        //var c = GetPlayerColor(state.Players0.FirstOrDefault(p => p.Role.Name == "Doctor"));

        SetActivePlayersSilent(state.Players.Select(p => new ActivePlayer(p, OnActivePlayerChange)
        {
            TextColor = GetPlayerColor(p)
        }).OrderBy(p => p.Player.Group.Name).ThenBy(p => p.Player.Role.Rank));
    }

    private async Task TellTheNews(State state)
    {
        if (!state.HasNews)
        {
            await Interact(new Interaction 
            { 
                Message = $"Hello City!", 
                State = state 
            });
        }
        else
        {
            await Interact(new Interaction 
            { 
                Message = $"Killed: {state.LatestNews.Killed.SJoin(", ")}", 
                Killed = state.LatestNews.Killed,
                State = state 
            });

            SetActivePlayersSilent(ActivePlayers.Where(p => state.Players.Contains(p.Player)).ToArray());

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

        await Interact(new Interaction
        {
            Message = $"GameEnd, the winner is {winnerGroup.Name}",
            State = state
        });
    }

    public async Task<Player[]> AskCityToSelect(State state, CityAction action)
    {
        var result = await Interact(new Interaction
        {
            Message = $"City selects somebody to kill{(action.IsSkippable() ? " or skip" : "")}",
            Selection = (action.IsSkippable() ? 0 : 1, 1),
            State = state
        });

        return result.Selected;
    }

    public async Task<Player[]> GetNeighbors(State state, Player player, Action action)
    {
        var result = await Interact(new Interaction
        {
            Message = $"Select {player} neighbors to kill",
            Selection = (2, 2),
            State = state
        });

        return result.Selected;
    }

    public async Task<Player[]> AskToSelect(State state, Player player, Action action)
    {
        var result = await Interact(new Interaction
        {
            Message = $"{player}, whom would you like to select{(action.IsSkippable() ? " or skip" : "")}?",
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
            Message = "City, wake up please",
            State = state
        });
    }

    private async Task AskCityToFallAsleep(State state)
    {
        await Interact(new Interaction
        {
            Message = "City, fall asleep please",
            State = state
        });
    }
}
