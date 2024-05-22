using Mafia.Extensions;
using Mafia.Model;

namespace Mafia.Hosts;

public class VicinoManiacPartyRatingHost : VicinoRatingHost
{
    protected override string[] GetDoctorGoodHeal() => GetCivilian();
    protected override string[] GetManiacGoodKill() => GetCivilian().Concat(GetMafia()).ToArray();
    protected override string[] GetKamikazeGoodKill() => GetManiac().Concat(GetFan()).Concat(GetMafia()).ToArray();
    protected virtual string[] GetFan() => ["Fan", "Фанат"];

    public VicinoManiacPartyRatingHost(Replay replay) : base(replay)
    {        
    }

    public override void AddManiacBonus(State state)
    {
        // skip maniac bonus
    }

    public override async Task NotifyGameEnd(State state, Group winner)
    {
        await base.NotifyGameEnd(state, winner);

        //var fan = GetFan();
        //var goodManiacKill = GetManiacGoodKill();

        // fan        
        //var isFanKillMaster = state.News.SelectMany(n => n.AllSelects().Where(s => s.Who != null).Select(s => (n, s)))
        //    .Where(s => fan.Contains(s.s.Who.Role.Name))
        //    .Select(s => (s.s.Who, count: s.s.Whom.Where(p => s.n.FactKills.Contains(p) && goodManiacKill.Contains(p.Role.Name)).Count()))
        //    .GroupBy(s => s.Who.Role.Name)
        //    .Select(gv => gv.Sum(v => v.count))
        //    .Any(count => count >= ManiacKillsBonusCount);

        //if (isFanKillMaster)
        //    state.Players0
        //        .Where(p => fan.Contains(p.Role.Name))
        //        .ForEach(p => ratings[p.User!.Nick].Add(RatingCase.KillMaster));
    }
}
