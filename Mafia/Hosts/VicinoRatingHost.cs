using Mafia.Extensions;
using Mafia.Model;

namespace Mafia.Hosts;

public class VicinoRatingHost : ReplayHost, IRating
{
    private Dictionary<string, int> ratings = new();

    private const int winnerBonus = 3;
    private const int aliveWinnerBonus = 2;
    private const int deathBonus = 1;

    private const int maniacBonus = 3;
    private const int kamikazeBonus = 3;
    private const int doctorBonus = 3;

    private const int maniacKillsBonusCount = 3;
    private const int doctorHealsBonusCount = 3;

    private readonly string[] goodHeal = [ "Maniac", "Commissar", "Doctor", "Civilian", "Маньяк", "Комиссар", "Доктор", "Мирный" ];
    private readonly string[] mafia = ["Don", "Bum", "Mafia", "Дон", "Бомж", "Мафия"];
    private readonly string[] kamikaze = ["Kamikaze", "Камикадзе"];
    private readonly string[] doctor = ["Doctor", "Доктор"];
    private readonly string[] maniac = ["Maniac", "Маньяк"];

    public (string, int)[] GetRatings() => ratings.Select(r => (r.Key, r.Value)).ToArray();

    public VicinoRatingHost(Replay replay) : base(replay)
    {
    }


    public override Task NotifyGameEnd(State state, Group winner)
    {
        bool IsAlive(string nick) => state.Players.Any(p => p.User.Nick == nick);
        bool IsWinner(string role) => winner.AllDeepRoles().Any(r => r.Name == role);

        ratings = state.Users0.ToDictionary(u => u.Nick, u => 0);

        // winner
        winner.AllDeepRoles()
            .SelectMany(r => replay.Players.Where(p => p.role == r.Name))
            .ForEach(p => ratings[p.nick] += winnerBonus);

        // alive & death
        state.Users0
            .SelectMany(u => replay.Players.Where(p => p.nick == u.Nick))
            .ForEach(p => ratings[p.nick] += IsAlive(p.nick) && IsWinner(p.role) ? aliveWinnerBonus : deathBonus);

        // maniac
        state.News.SelectMany(n => n.AllSelects().Select(s => (n, s)))
            .Where(s => s.s.Who != null && maniac.Contains(s.s.Who.Role.Name))
            .Select(s => (s.s.Who, count: s.s.Whom.Where(p => s.n.FactKills.Contains(p)).Count()))
            .GroupBy(s => s.Who)
            .Select(gv => (nick: gv.Key.User.Nick, count: gv.Sum(v => v.count)))
            .Where(v => v.count >= maniacKillsBonusCount)
            .ForEach(p => ratings[p.nick] += maniacBonus);

        // doctor
        state.News.SelectMany(n => n.AllSelects().Select(s => (n, s)))
            .Where(s => s.s.Who != null && doctor.Contains(s.s.Who.Role.Name))
            .Select(s => (s.s.Who, count: s.s.Whom.Where(p => s.n.FactHeals.Contains(p) && goodHeal.Contains(p.Role.Name)).Count()))
            .GroupBy(s => s.Who)
            .Select(gv => (nick: gv.Key.User.Nick, count: gv.Sum(v => v.count)))
            .Where(v => v.count >= doctorHealsBonusCount)
            .ForEach(p => ratings[p.nick] += doctorBonus);

        // kamikaze
        state.News.SelectMany(n => n.AllSelects())
            .Where(s => s.Who != null && kamikaze.Contains(s.Who.Role.Name))
            .Where(s => s.Whom.Any(p => mafia.Contains(p.Role.Name)))
            .GroupBy(s => s.Who)
            .Select(gv => gv.Key.User.Nick)
            .ForEach(nick => ratings[nick] += kamikazeBonus);

        return base.NotifyGameEnd(state, winner);
    }
}
