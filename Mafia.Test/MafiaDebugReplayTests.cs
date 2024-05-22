using System.Diagnostics;
using Mafia.Model;
using Mafia.Services;
using Mafia.Test.Base;
using Microsoft.Extensions.DependencyInjection;

namespace Mafia.Test;

public class MafiaDebugReplayTests : MafiaTestsBase
{
    [Fact]
    public async Task Vicino_ru_Classic15()
    {
        var replay = new Replay()
        {
            Players = [("U00", "Don"), ("U01", "Bum"), ("U02", "Maniac"), ("U03", "Commissar"), ("U04", "Doctor"), ("U05", "Kamikaze"), ("U06", "Mafia"), ("U07", "Mafia"), ("U08", "Civilian"), ("U09", "Civilian"), ("U10", "Civilian"), ("U11", "Civilian"), ("U12", "Civilian"), ("U13", "Civilian"), ("U14", "Civilian")],
            Selections = [[(-1, []), (-1, [0])], [(1, [5]), (2, [8]), (4, [4]), (3, [13]), (5, [])], [(-1, []), (-1, [7])], [(1, [9]), (2, [10]), (4, [14]), (3, [6])], [(-1, []), (-1, [1])], [(6, [12]), (2, [11]), (4, [12]), (3, [6])], [(-1, []), (-1, [3])], [(6, [12]), (2, [13]), (4, [13])], [(-1, []), (-1, [6])], [(2, [13]), (4, [2])], [(-1, []), (-1, [])], [(2, [4]), (4, [14])]]
        };

        var provider = CreateReplayTest("mafia-vicino.json", replay);

        var game = provider.GetRequiredService<Game>();
        await game.Start();

        var state = game.State;
        var city = provider.GetRequiredService<ICity>().City;
        var referee = provider.GetRequiredService<Referee>();
        var ratings = await referee.GetRatings(replay, city);
        
        foreach (var p in state.Players0)
        {
            var rating = ratings.Single(r => r.nick == p.User.Nick).rating;
            Debug.WriteLine($"{p.User.Nick} ({p.Role.Name}): {rating}");
        }
    }

}
