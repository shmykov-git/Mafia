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
            [(-1, [6]), (0, [4]), (2, [5]), (4, [5]), (3, [2]), (-1, [8]), (0, [2]), (2, [5]), (3, [5]), (-1, [0]), (1, [9]), (3, [9]), (-1, [1])]
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

    [Fact]
    public async Task Vicino_ru_FullDifficult20()
    {
        // проверить, что комиссар не просыпается, когда его убили

        (int, int[])[][] validGames =
        [
            [(-1, [8]), (8, []), (0, [5]), (2, [19]), (3, [1]), (6, [5]), (4, [16]), (-1, [5]), (0, [3]), (2, [17]), (3, [0]), (6, [0]), (4, [12]), (-1, [7]), (7, []), (0, [17]), (2, [4]), (6, [2]), (-1, [9]), (0, [16]), (2, [19]), (6, [16]), (4, [14]), (-1, [2]), (0, [16]), (6, [19]), (4, [19]), (-1, [0]), (10, [12]), (6, [18]), (4, [15]), (-1, [15]), (10, [4]), (6, [4]), (4, [10]), (-1, [11]), (10, [19]), (6, [14]), (4, [6]), (-1, [10])]
        ];

        foreach (var selections in validGames)
        {
            var provider = CreateTest("mafia-vicino-ru.json", options =>
            {
                options.Debug = false;
                options.TestRoles = [("Дон", 1), ("Бомж", 1), ("Проститутка", 1), ("Маньяк", 1), ("Комиссар", 1), ("Сержант", 1), ("Доктор", 1), ("Камикадзе", 1), ("Шахид", 1), ("Мафия", 3), ("Мирный", 8)];
                options.Selections = selections;
            });
            var game = provider.GetRequiredService<Game>();

            await game.Start();
        }
    }    
}
