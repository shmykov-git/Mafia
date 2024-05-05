using System.Diagnostics;
using Mafia.Extensions;
using Mafia.Model;

namespace Run;

/// <summary>
/// todo: view code for host
/// </summary>
public class DebugHost : IHost
{
    private Random rnd;
    private readonly City city;

    public DebugHost(int seed, City city)
    {
        rnd = new Random(seed);
        this.city = city;
    }

    public Player[] GetPlayers()
    {
        var nMax = 20;
        var users = Enumerable.Range(1, nMax + 1).Select(i => new User { Nick = $"Nick{i}" }).ToArray();

        var listP = users.ToList();

        var n = 15;
        var gamePlayers = Enumerable.Range(1, n + 1).Select(_ =>
        {
            var i = rnd.Next(listP.Count);
            var player = listP[i];
            listP.RemoveAt(i);
            return player;
        }).ToArray();

        var civilianRole = city.GetRole("Civilian");
        var mafiaRole = city.GetRole("Mafia");

        var nn = n - city.AllRoles().Count();
        var nMafia = nn / 3;
        var nCivilian = nn - nMafia;

        var mafias = Enumerable.Range(0, nMafia).Select(_ => mafiaRole);
        var civilians = Enumerable.Range(0, nCivilian).Select(_ => civilianRole);

        var gameRoles = city.AllRoles().Concat(mafias).Concat(civilians).ToArray();

        gameRoles.Shaffle(17, rnd);

        var players = gameRoles.Select((r, i) => new Player { User = gamePlayers[i], Role = gameRoles[i], Group = city.GetGroup(gameRoles[i]) }).ToArray();

        return players;
    }

    public Player AskToSelect(State state, Player player)
    {
        AskToWakeUp(state, player);

        Debug.WriteLine($"Whom would like to select, {player}?");

        AskToFallAsleep(state, player);

        return null;
    }

    public bool AskToSkip(State state, Player player)
    {
        throw new NotImplementedException();
    }




    private void AskToWakeUp(State state, Player player)
    {
        if (state.IsNight)
            Debug.WriteLine($"Wake up please, {player}?");
    }

    private void AskToFallAsleep(State state, Player player)
    {
        if (state.IsNight)
            Debug.WriteLine($"Fall asleep please, {player}?");
    }
}
