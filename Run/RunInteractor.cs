using System.Diagnostics;
using Mafia.Models;
using Mafia.Services;

namespace Mafia.Interactors;

public class RunInteractor : IInteractor
{
    private Game game;
    private Random rnd;

    public RunInteractor(int seed)
    {
        rnd = new Random(seed);
    }

    private List<Player> alivePlayers => game.alivePlayers;

    public Player[] GetPlayers(Model model)
    {
        var nMax = 20;
        var users = Enumerable.Range(1, nMax + 1).Select(i => new User { Name = $"User {i}", Nick = $"Nick{i}" }).ToArray();

        var listP = users.ToList();

        var n = 15;
        var gamePlayers = Enumerable.Range(1, n + 1).Select(_ =>
        {
            var i = rnd.Next(listP.Count);
            var player = listP[i];
            listP.RemoveAt(i);
            return player;
        }).ToArray();

        var civilianRole = model.Groups.Single(g => g.IsCivilian).Roles!.Last();
        var mafiaRole = model.Groups.Single(g => g.IsMafia).Roles!.Last();

        var nn = n - model.Roles.Length;
        var nMafia = nn / 3;
        var nCivilian = nn - nMafia;

        var mafias = Enumerable.Range(0, nMafia).Select(_ => mafiaRole);
        var civilians = Enumerable.Range(0, nCivilian).Select(_ => civilianRole);

        var roles = model.Roles.Concat(mafias).Concat(civilians).ToArray();
        SelectAct? GetAct(string role) => model.SelectActs.FirstOrDefault(a=>a.Role == role);

        foreach (var _ in Enumerable.Range(0, rnd.Next(17)))
        {
            var i = rnd.Next(roles.Length);
            var j = rnd.Next(roles.Length);
            (roles[i], roles[j]) = (roles[j], roles[i]);
        }

        var players = roles.Select((r, i) => new Player { User = gamePlayers[i], Role = roles[i], ParticipantGroup = model.GetGroupByRole(roles[i]), SelectAct = GetAct(roles[i]) }).ToArray();

        return players;
    }

    public string GameFileName => "mafia.json";

    public void ApplyGame(Game game)
    {
        this.game = game;
    }

    public Player[] CitySelect(bool skippable) => NeedSkip(skippable) ? [] : [alivePlayers[rnd.Next(alivePlayers.Count)]];

    public Player[] Select(Player p, ICollection<Player> players, bool skippable)
    {
        if (p.Role == "Doctor" && players.Contains(p))
        {
            return [p];
        }

        var otherGroupPlayers = p.ParticipantGroup == null 
            ? players.Where(ap => ap != p).ToArray() 
            : players.Where(ap => ap.ParticipantGroup != p.ParticipantGroup).ToArray();

        return NeedSkip(skippable) ? [] : [otherGroupPlayers[rnd.Next(otherGroupPlayers.Length)]];
    }

    public Player[] DoubleKillOnDeath(Player p)
    {
        var ind = alivePlayers.IndexOf(p);
        var i = (ind - 1 + alivePlayers.Count) % alivePlayers.Count;
        var j = (ind + 1) % alivePlayers.Count;

        return [alivePlayers[i], alivePlayers[j]];
    }

    public void Info(string message)
    {
        Debug.WriteLine($"Info| {message}");
    }

    public void Tells(string message)
    {
        Debug.WriteLine($"Tell| {message}");
    }

    public void Log(string message)
    {
        Debug.WriteLine(message);
    }

    private bool NeedSkip(bool skippable) => skippable ? rnd.NextDouble() < 0.1 : false;

}
