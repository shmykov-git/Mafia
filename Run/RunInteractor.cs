using System.Diagnostics;
using Mafia.Models;
using Mafia.Services;

namespace Mafia.Interactors;

public class RunInteractor : IInteractor
{
    private Game game;
    private Random rnd;

    public RunInteractor(int seed = 0)
    {
        rnd = new Random(seed);
    }

    private List<Player> alivePlayers => game.alivePlayers;

    public Player[] GetPlayers(Model model)
    {
        var nMax = 20;
        var rnd = new Random();
        var users = Enumerable.Range(1, nMax + 1).Select(i => new User { Name = $"User {i}", Nick = $"Nick{i}" }).ToArray();

        var listP = users.ToList();

        var n = 10;
        var gamePlayers = Enumerable.Range(1, n + 1).Select(_ =>
        {
            var i = rnd.Next(listP.Count);
            var player = listP[i];
            listP.RemoveAt(i);
            return player;
        }).ToArray();

        var roles = model.Roles;

        foreach (var _ in Enumerable.Range(0, rnd.Next(17)))
        {
            var i = rnd.Next(roles.Length);
            var j = rnd.Next(roles.Length);
            (roles[i], roles[j]) = (roles[j], roles[i]);
        }

        var players = roles.Select((r, i) => new Player { User = gamePlayers[i], Role = roles[i] }).ToArray();

        return players;
    }

    public string ModelFileName => "mafia.json";

    public void ApplyGame(Game game)
    {
        this.game = game;
    }

    private bool NeedSkip(bool skippable) => skippable ? rnd.NextDouble() < 0.1 : false;

    public Player[] CitySelect(bool skippable) => NeedSkip(skippable) ? [] : [alivePlayers[rnd.Next(alivePlayers.Count)]];

    public Player[] Select(Player p, bool skippable, Act act) => NeedSkip(skippable) ? [] : [alivePlayers[rnd.Next(alivePlayers.Count)]];

    public Player[] DoubleSelect(Player p)
    {
        var ind = alivePlayers.IndexOf(p);
        var i = (ind - 1 + alivePlayers.Count) % alivePlayers.Count;
        var j = (ind + 1) % alivePlayers.Count;

        return [alivePlayers[i], alivePlayers[j]];
    }

    public void TellInformation(string message)
    {
        Debug.WriteLine(message);
    }
}
