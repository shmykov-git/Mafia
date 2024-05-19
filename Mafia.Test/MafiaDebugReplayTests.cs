using Mafia.Model;
using Mafia.Test.Base;
using Microsoft.Extensions.DependencyInjection;

namespace Mafia.Test;

public class MafiaDebugReplayTests : MafiaTestsBase
{
    [Fact]
    public async Task Vicino_ru_Difficult15()
    {
        var provider = CreateReplayTest("mafia-vicino-ru.json", new Replay()
        {
            Selections = [[(-1, [12])], [(0, [14]), (2, [11]), (3, [13]), (5, [0]), (4, [3])], [(-1, [2])], [(0, [10]), (3, [4]), (5, [9])], [(-1, [11])], [(0, [5]), (3, [9]), (5, [6])], [(-1, [3])]],
            Players = [("U00", "Дон"), ("U01", "Бомж"), ("U02", "Проститутка"), ("U03", "Маньяк"), ("U04", "Комиссар"), ("U05", "Доктор"), ("U06", "Мафия"), ("U07", "Мафия"), ("U08", "Мирный"), ("U09", "Мирный"), ("U10", "Мирный"), ("U11", "Мирный"), ("U12", "Мирный"), ("U13", "Мирный"), ("U14", "Мирный")]
        });

        var game = provider.GetRequiredService<Game>();
        await game.Start();
    }

}
