using System.Data;
using System.Diagnostics;
using Mafia.Executions;
using Mafia.Extensions;
using Mafia.Libraries;

namespace Mafia.Model;

public class State
{
    public required IHost Host { get; set; }
    public required City City { get; set; }
    public required List<DailyNews> News { get; set; }
    public required Replay Replay { get; set; }
    public DailyNews YesterdayNews => News.Count < 3 ? new DailyNews() : News[^3];
    public DailyNews LatestDayNews => IsDay ? LatestNews : (News.Count < 2 ? new DailyNews() : News[^2]);
    public DailyNews LatestNews => News.Count < 1 ? new DailyNews() : News[^1];
    public bool HasNews => News.Count > 0;

    public Player[] AllFactKills() => News.SelectMany(dn => dn.FactKills).ToArray();

    public int DayNumber { get; set; }
    public bool IsActive { get; set; }
    public bool Stopping { get; set; }
    public bool RollingBack { get; set; }
    public bool IsFirstDay => DayNumber == 1;

    public bool IsDay { get; set; }
    public bool IsNight { get => !IsDay; set => IsDay = !value; }
    public bool IsMorning { get; set; }
    public bool IsEvening { get => !IsMorning; set => IsMorning = !value; }

    public User[] Users0 { get; set; }
    public required Player[] Players0 { get; set; }
    public required List<Player> Players { get; set; }

    public void Rollback()
    {
        RollingBack = true;
    }

    public IEnumerable<Select> AllSelects() => News.SelectMany(dn=>dn.AllSelects());

    /// <summary>
    /// Нужно чтобы в списке действий было хотя бы одно, которое не блокируется условиями текущего процесса
    /// Используется при вычислении максимального незалоченного ранга. Например, если Дон залочен, то стреляет Бомж - рангом ниже
    /// Не используется для вычисления незаблокированных людей в группе, т.к. на карте есть NoLocked condition, который это сделает (и запишет новость)
    /// Должно быть использовано только для расчета условий или в операциях
    /// </summary>
    public bool IsActiveAllowed(Player player) => player.Role.AllActions()
        .Select(a => (a, needCheck: a.AllConditions().Intersect(Values.ActiveConditions).ToArray()))
        .Any(v => v.needCheck.Length == 0 || v.needCheck.All(name => v.a.CheckCondition(name, this, player).NoInteractionResult()));

    public bool IsAlive(Player player) => Players.Contains(player);
    public bool IsAliveRole(Role role) => Players.Any(p => p.Role == role);
    public bool HasImmunity(User user) => News.SelectMany(n => n.AllSelects()).Any(s => Values.ImmunityConditions.Contains(s.Operation) && s.UserWhom.Contains(user));
    public bool DoesDoctorHaveThanks() => LatestNews.FactHeals.Length > 0;

    // todo: прояснить действия путаны в разных ситуациях
    public bool DoesSomebodyExceptDoctorSkipKills()
    {
        // блокирует ли проститутка убийство того куда она идет
        // если она его только лочит, то блокирует ли его действие, например камикадзе или шахида
        // if you have locker in the team you cannot be locked to kill somebody

        return LatestNews.GetLockedKillers().Length > 0;
    }

    public Group[] GetKillerGroups() => Players.Select(p => p.Group).Distinct().Where(g => g.HasAnyOperation(Values.KillOperations)).ToArray();

    public bool IsSelfSelected(Player player) => News.Select(ps => ps).Any(ops => ops.Selects?.Any(s => s.Who == player && s.Whom.Contains(player)) ?? false);
    // 
    public Player[] GetGroupActivePlayers(Group group) => Players.Where(p => p.Group == group).GroupBy(p=>p.Role).Select(gr=>gr.MinBy(p=>p.Id)!).OrderBy(p=>p.Role.Rank).ToArray();
    public Player[] GetTeam(Player player) => Players.Where(p => player.Group.Roles!.Contains(p.Role)).ToArray();
    public Player[] GetTeamOthers(Player player) => Players.Where(p => p != player && player.Group.Roles!.Contains(p.Role)).ToArray();
    public Player[] GetOtherTeams(Player player) => Players.Where(p => p.Group != player.Group).ToArray();
    public int GetTeamSeniorRank(Player player) => GetTeam(player).Where(IsActiveAllowed).MinBy(p => p.Role.Rank)?.Role.Rank ?? -1;
    public Player[] GetNeighborPlayers(Player player)
    {
        var n = Players.Count;
        var ind = Players.IndexOf(player);
        var i = (ind - 1 + n) % n;
        var j = (ind + 1) % n;

        return [Players[i], Players[j]];

    }

    public User[] GetExceptUsers(string operation)
    {
        if (Values.KillOperations.Contains(operation))
        {
            if (IsFirstDay)
            {
                return Users0.Where(HasImmunity).ToArray();
            }
        }

        return [];
    }

    public User[] GetExceptUsers(Player player, string operation)
    {
        if (Values.HealOperations.Contains(operation))
        {
            if (IsFirstDay)
                return [];

            List<Player> except = new();

            if (City.IsRuleForRoleAccepted(RuleName.EvenDoctorDays, player.Role))
            {
                var select = YesterdayNews.AllSelects().SingleOrDefault(s => s.Who == player);

                if (select?.Whom.Length > 0)
                    except.AddRange(select.Whom.Intersect(Players));
            }

            if (City.IsRuleForRoleAccepted(RuleName.DoctorOnceSelfHeal, player.Role))
            {
                if (AllSelects().Where(s => s.Who == player).Any(s => s.Whom.Contains(player)))
                    except.Add(player);
            }

            return except.Select(p => p.User).ToArray();
        }

        return GetExceptUsers(operation);
    }

    public Player[] GetLatestFactKills()
    {
        var locks = LatestNews.AllKnownLocks(this);
        var kills = LatestNews.AllKnownKills(this)
            .Where(s => !locks.Any(l => l.Whom.Contains(s.Who)))
            .SelectMany(k => k.Whom)
            .GroupBy(v => v).Select(gv => (p: gv.Key, c: gv.Count())).ToArray();

        var heals = LatestNews.AllKnownHeals(this)
            .Where(s => !locks.Any(l => l.Whom.Contains(s.Who)))
            .SelectMany(k => k.Whom)
            .GroupBy(v => v).Select(gv => (p: gv.Key, c: gv.Count())).ToArray();

        var factKills = City.GetRule(RuleName.HealSingleKill).Accepted
            ? kills.Where(k => heals.FirstOrDefault(h => h.p == k.p).c < k.c).Select(k => k.p).ToArray()
            : kills.Where(k => heals.FirstOrDefault(h => h.p == k.p).c == 0).Select(k => k.p).ToArray();

        return City.NightEvents.Select(name => factKills.SingleOrDefault(k => k.Group.Name == name)).Where(k => k != null).ToArray();
    }

    public void DoKnowAllLatestWhom() => LatestNews.DoKnowAllWhom(this);
}
