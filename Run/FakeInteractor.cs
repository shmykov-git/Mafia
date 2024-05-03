using System.Diagnostics;
using Mafia.Models;
using Mafia.Modules;

namespace Mafia.Interactors;

public class FakeInteractor : IInteractor
{
    private readonly Game game;
    private Random rnd;

    public FakeInteractor(Game game, int seed = 0)
    {
        this.game = game;
        rnd = new Random(seed);
    }

    private List<Player> alivePlayers => game.alivePlayers;

    public bool AskCityToSkip()
    {
        return rnd.NextDouble() < 0.1;
    }

    public Player[] Select(Player p) => [alivePlayers[rnd.Next(alivePlayers.Count)]];

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
