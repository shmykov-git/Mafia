using System.Data;
using Mafia.Extensions;
using Mafia.Libraries;


namespace Mafia.Model;

public class State
{
    public required IHost Host { get; set; }
    public required City City { get; set; }
    public required List<DailyNews> News { get; set; }
    public DailyNews YesterdayNews => News.Count < 3 ? new DailyNews() : News[^3];
    public DailyNews LatestDayNews => IsDay ? LatestNews : (News.Count < 2 ? new DailyNews() : News[^2]);
    public DailyNews LatestNews => News.Count < 1 ? new DailyNews() : News[^1];
    public bool HasNews => News.Count > 0;

    public int DayNumber { get; set; }
    public bool IsActive { get; set; }
    public bool Stopping { get; set; }
    public bool IsDay { get; set; }
    public bool IsNight { get => !IsDay; set => IsDay = !value; }

    public required Player[] Players0 { get; set; }
    public required List<Player> Players { get; set; }

    public IEnumerable<Select> AllSelects() => News.SelectMany(dn=>dn.AllSelects());

    /// <summary>
    /// Нужно чтобы в списке действий было хотя бы одно, которое не блокируется условиями текущего процесса
    /// </summary>
    public bool IsCurrentlyAllowed(Player player) => player.Role.AllActions()
        .Select(a => (a, intersection: a.AllConditions().Intersect(Values.ActiveConditions).ToArray()))
        .Any(v => v.intersection.Length == 0 || v.intersection.All(name => v.a.CheckCondition(name, this, player).NoInteractionResult()));

    public bool IsAlive(Player player) => Players.Contains(player);
    public bool IsAliveRole(Role role) => Players.Any(p => p.Role == role);

    public bool IsSelfSelected(Player player) => News.Select(ps => ps).Any(ops => ops.Selects?.Any(s => s.Who == player && s.Whom.Contains(player)) ?? false);
    public Player[] GetGroupActivePlayers(Group group) => Players.Where(IsCurrentlyAllowed).Where(p => p.Group == group).GroupBy(p=>p.Role).Select(gr=>gr.MinBy(p=>p.Id)).OrderBy(p=>p.Role.Rank).ToArray();
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

    public Player[] GetExceptPlayers(Player player)
    {
        List<Player> except = new();

        if (City.IsRuleForRoleAccepted(RuleName.EvenDoctorDays, player.Role))
        {
            var select = YesterdayNews.AllSelects().SingleOrDefault(s => s.Who == player);
            
            if (select?.Whom.Length > 0)
                except.AddRange(select.Whom.Intersect(Players));
        }

        if (City.IsRuleForRoleAccepted(RuleName.DoctorOnceSelfHeal, player.Role))
        {
            if (AllSelects().Where(s=>s.Who == player).Any(s => s.Whom.Contains(player)))
                except.Add(player);
        }

        return except.ToArray();
    }
}
