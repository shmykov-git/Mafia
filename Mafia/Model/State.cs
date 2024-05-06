using System.Data;

namespace Mafia.Model;

public class State
{
    public required IHost Host { get; set; }
    public required City City { get; set; }
    public required List<DailyNews> News { get; set; }
    public DailyNews LatestNews => News[^1];

    public int DayNumber { get; set; }
    public bool IsDay { get; set; }
    public bool IsNight { get => !IsDay; set => IsDay = !value; }

    public required Player[] Players0 { get; set; }
    public required List<Player> Players { get; set; }


    // Нужно чтобы в списке действий было хотя бы одно, которое не блокируется условиями текущего процесса
    public bool IsCurrentlyAllowed(Player player) => player.Role.AllActions()
        .Select(a => (a, intersection: a.AllConditions().Intersect(City.CurrentProcessConditions).ToArray()))
        .Any(v => v.intersection.Length == 0 || v.intersection.All(name => v.a.CheckCondition(name, this, player)));

    public bool IsSelfSelected(Player player) => News.Select(ps => ps).Any(ops => ops.Selects?.Any(s => s.Who == player && s.Whom.Contains(player)) ?? false);
    public Player[] GetGroupPlayers(Group group) => Players.Where(p => p.Group == group).ToArray();
    public Player[] GetTeam(Player player) => Players.Where(p => player.Group.Roles!.Contains(p.Role)).ToArray();
    public Player[] GetTeamOthers(Player player) => Players.Where(p => p != player && player.Group.Roles!.Contains(p.Role)).ToArray();
    public Player[] GetOtherTeams(Player player) => Players.Where(p => p.Group != player.Group).ToArray();
    public int GetTeamSeniorRank(Player player) => GetTeam(player).Where(IsCurrentlyAllowed).MinBy(p => p.Role.Rank)?.Role.Rank ?? -1;
    public Player[] GetNeighborPlayers(Player player)
    {
        var n = Players.Count;
        var ind = Players.IndexOf(player);
        var i = (ind - 1 + n) % n;
        var j = (ind + 1) % n;

        return [Players[i], Players[j]];

    }
}
