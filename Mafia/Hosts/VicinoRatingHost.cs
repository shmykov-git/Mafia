using System.Numerics;
using Mafia.Extensions;
using Mafia.Model;

namespace Mafia.Hosts;

public class VicinoRatingHost : ReplayHost, IRating
{
    protected Dictionary<string, List<RatingCase>> ratings = new();

    protected virtual int GetBonus(RatingCase ratingCase) => ratingCase switch
    {
        RatingCase.Winner => 3,
        RatingCase.Loser => 1,
        RatingCase.Alive => 2,
        RatingCase.HealMaster => 3,
        RatingCase.KillMaster => 3,
        _ => throw new NotImplementedException()
    };

    protected virtual int ManiacKillsBonusCount => 3;
    protected virtual int DoctorHealsBonusCount => 3;

    protected virtual string[] GetCivilian() => ["Commissar", "Doctor", "Kamikaze", "Civilian", "Комиссар", "Доктор", "Камикадзе", "Мирный"];
    protected virtual string[] GetMafia() => ["Don", "Bum", "Mafia", "Дон", "Бомж", "Мафия"];
    protected virtual string[] GetDoctorGoodHeal() => GetCivilian().Concat(GetManiac()).ToArray();
    protected virtual string[] GetManiacGoodKill() => GetMafia();
    protected virtual string[] GetKamikazeGoodKill() => GetMafia();
    protected virtual string[] GetKamikaze() => ["Kamikaze", "Камикадзе"];
    protected virtual string[] GetDoctor() => ["Doctor", "Доктор"];
    protected virtual string[] GetManiac() => ["Maniac", "Маньяк"];

    public (string, int, RatingCase[])[] GetRatings() => ratings.Select(r => (r.Key, r.Value.Select(GetBonus).Sum(), r.Value.ToArray())).ToArray();

    public VicinoRatingHost(Replay replay) : base(replay)
    {
    }

    public virtual void AddManiacBonus(State state)
    {
        var goodManiacKill = GetManiacGoodKill();
        var maniac = GetManiac();

        state.News.SelectMany(n => n.AllSelects().Where(s => s.Who != null).Select(s => (n, s)))
            .Where(s => maniac.Contains(s.s.Who.Role.Name))
            .Select(s => (s.s.Who, count: s.s.Whom.Where(p => s.n.FactKills.Contains(p) && goodManiacKill.Contains(p.Role.Name)).Count()))
            .GroupBy(s => s.Who)
            .Select(gv => (nick: gv.Key.User!.Nick, count: gv.Sum(v => v.count)))
            .Where(v => v.count >= ManiacKillsBonusCount)
            .ForEach(p => ratings[p.nick].Add(RatingCase.KillMaster));
    }

    public virtual void AddKamikazeBonus(State state)
    {
        var goodKamikazeKill = GetKamikazeGoodKill();
        var kamikaze = GetKamikaze();

        state.News.SelectMany(n => n.AllSelects().Where(s => s.Who != null).Select(s => (n, s)))
            .Where(s => kamikaze.Contains(s.s.Who.Role.Name))
            .Where(s => s.s.Whom.Any(p => s.n.FactKills.Contains(p) && goodKamikazeKill.Contains(p.Role.Name)))
            .GroupBy(s => s.s.Who)
            .Select(gv => gv.Key.User!.Nick)
            .ForEach(nick => ratings[nick].Add(RatingCase.KillMaster));
    }

    public virtual void AddDoctorBonus(State state)
    {
        var goodDoctorHeal = GetDoctorGoodHeal();
        var doctor = GetDoctor();

        state.News.SelectMany(n => n.AllSelects().Where(s => s.Who != null).Select(s => (n, s)))
            .Where(s => doctor.Contains(s.s.Who.Role.Name))
            .Select(s => (s.s.Who, count: s.s.Whom.Where(p => s.n.FactHeals.Contains(p) && goodDoctorHeal.Contains(p.Role.Name)).Count()))
            .GroupBy(s => s.Who)
            .Select(gv => (nick: gv.Key.User!.Nick, count: gv.Sum(v => v.count)))
            .Where(v => v.count >= DoctorHealsBonusCount)
            .ForEach(p => ratings[p.nick].Add(RatingCase.HealMaster));
    }

    public override async Task NotifyGameEnd(State state, Group winner)
    {
        await base.NotifyGameEnd(state, winner);

        bool IsAlive(string nick) => state.Players.Any(p => p.User!.Nick == nick);

        ratings = state.Users0.ToDictionary(u => u.Nick, u => new List<RatingCase>());
        var winnerRoles = winner.AllDeepRoles().Select(r => r.Name).ToArray();

        // winner (alive), loser
        replay.Players.ForEach(p =>
        {
            if (winnerRoles.Contains(p.role))
            {
                ratings[p.nick].Add(RatingCase.Winner);

                if (IsAlive(p.nick))
                    ratings[p.nick].Add(RatingCase.Alive);
            }
            else
            {
                ratings[p.nick].Add(RatingCase.Loser);
            }
        });

        AddManiacBonus(state);
        AddDoctorBonus(state);
        AddKamikazeBonus(state);
    }
}
