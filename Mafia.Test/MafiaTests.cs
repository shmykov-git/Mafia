using Mafia.Test.Base;
using Microsoft.Extensions.DependencyInjection;

namespace Mafia.Test;

public class MafiaTests : MafiaTestsBase
{
    [Fact]
    public async Task Vicino_ru_Difficult15()
    {
        (int, int[])[][] validGames =
        [
            [(-1, [4]), (0, [8]), (2, [11]), (3, [8]), (5, [14]), (-1, [14]), (0, [11]), (2, [3]), (5, [13]), (-1, [0]), (1, [3]), (2, [9]), (3, [2]), (5, [13]), (-1, []), (1, [13]), (5, [6]), (-1, [1]), (6, [12]), (5, [10]), (-1, [9]), (6, [5]), (5, [10])]
        ];

        foreach (var selections in validGames)
        {
            var provider = CreateTest("mafia-vicino-ru.json", options =>
            {
                options.Debug = true;
                options.TestRoles = [("Дон", 1), ("Бомж", 1), ("Проститутка", 1), ("Маньяк", 1), ("Комиссар", 1), ("Доктор", 1), ("Мафия", 2), ("Мирный", 7)];
                options.Selections = selections;
            });
            var game = provider.GetRequiredService<Game>();

            await game.Start();
        }
    }

    [Fact]
    public async Task Vicino_ru_Classic12()
    {
        // проверить, что комиссар не просыпается, когда его убили

        (int, int[])[][] validGames =
        [
            [(-1, [4]), (0, [3]), (2, [11]), (-1, [2]), (0, [10]), (-1, [1]), (0, [6]), (-1, [0]), (5, [7]), (-1, [8])]
        ];

        foreach (var selections in validGames)
        {
            var provider = CreateTest("mafia-vicino-ru.json", options =>
            {
                options.Debug = false;
                options.TestRoles = [("Дон", 1), ("Бомж", 1), ("Маньяк", 1), ("Комиссар", 1), ("Доктор", 1), ("Мафия", 1), ("Мирный", 6)];
                options.Selections = selections;
            });
            var game = provider.GetRequiredService<Game>();

            await game.Start();
        }
    }

}
