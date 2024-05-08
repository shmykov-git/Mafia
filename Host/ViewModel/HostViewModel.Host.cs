using System.Data;
using Host.Model;
using Mafia.Extensions;
using Mafia.Model;

namespace Host.ViewModel;

public partial class HostViewModel
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

    public void StartGame(State state)
    {
        ActivePlayers = state.Players.Select(p => new ActivePlayer(p, OnActivePlayerChange)).OrderBy(p => p.Player.Group.Name).ThenBy(p => p.Player.Role.Rank).ToArray();
        Changed(nameof(ActivePlayers));
    }

    private async Task TellTheNews(State state)
    {
        if (!state.HasNews)
        {
            await Interact(new Interaction 
            { 
                Message = $"Hello City! Players: {state.Players.SJoin(", ")}", 
                State = state 
            });
        }
        else
        {
            await Interact(new Interaction 
            { 
                Message = $"Killed: {state.LatestNews.Killed.SJoin(", ")}", 
                State = state 
            });

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
    }

    public async Task<bool> AskCityToSkip(State state)
    {
        var result = await Interact(new Interaction
        {
            Message = $"City, do you want to skip selection?",
            AskToSkip = true,
            State = state
        });

        return result.Skip;
    }

    public async Task<Player> AskCityToSelect(State state)
    {
        var result = await Interact(new Interaction
        {
            Message = $"City selects somebody to kill",
            Selection = (1, 1),
            State = state
        });

        return result.Selected[0];
    }

    public async Task<Player[]> GetNeighbors(State state, Player player)
    {
        var result = await Interact(new Interaction
        {
            Message = $"Select {player} neighbors to kill",
            Selection = (2, 2),
            State = state
        });

        return result.Selected;
    }

    public async Task<bool> AskToSkip(State state, Player player)
    {
        var result = await Interact(new Interaction
        {
            Message = $"{player}, do you want to skip selection?",
            AskToSkip = true,
            Player = player,
            State = state
        });

        return result.Skip;
    }

    public async Task<Player[]> AskToSelect(State state, Player player)
    {
        var result = await Interact(new Interaction
        {
            Message = $"{player}, whom would you like to select?",
            Selection = (1, 1),
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
