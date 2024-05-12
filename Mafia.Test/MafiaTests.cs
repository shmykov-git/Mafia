using Mafia.Model;
using Mafia.Test.Base;
using Mafia.Test.Hosts;
using Mafia.Test.Model;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Mafia.Test;

public class MafiaTests : MafiaTestsBase
{
    [Fact]
    public async Task Vicino_ru()
    {
        //0 - "Дон",
        //1 - "Бомж",
        //2 - "Маньяк",
        //3 - "Комиссар",
        //4 - "Доктор",
        //5 - "Мафия",
        //6 - "Мафия",
        //7 - "Мирный",
        //8 - "Мирный",
        //9 - "Мирный",
        //10 - "Мирный",
        //11 - "Мирный",
        //12 - "Мирный",
        //13 - "Мирный",

        void SetOptions(TestOptions options)
        {
            options.TestRoles = [("Дон", 1), ("Бомж", 1), ("Маньяк", 1), ("Комиссар", 1), ("Доктор", 1), ("Мафия", 2), ("Мирный", 7)];
            options.Selections =
            [
                (-1, [10]), // город -> мирный
                (0, [11]),  // дон -> мирный
                (2, [7]),   // маньяк -> мирный
                (4, [0]), 	// доктор -> дон
                (3, [0]), 	// комиссар -> дон
                (-1, [1]), 	// город -> бомж
                (0, [4]),	// дон -> доктор
                (2, [12]),	// маньяк -> мирный
                (4, [0]), 	// доктор -> дон
                (3, [2]),	// комиссар -> маньяк
                (-1, [8]),	// город -> мирный
                (0, [3]),	// дон -> комиссар
                (2, [0]),	// маньяк -> дон
                (3, [13]),	// комиссар -> мирный
                (-1, [2]),	// город -> маньяк
            ];
        }

        var provider = CreateTest("mafia-vicino-ru.json", SetOptions);
        var game = provider.GetRequiredService<Game>();
        await game.Start();
    }

    [Fact]
    public async Task Vicino_ru_Classic15()
    {
        (int, int[])[][] validGames =
        [
            [(-1, [4]), (0, [14]), (2, [10]), (3, [11]), (-1, []), (0, [8]), (2, [5]), (3, [13]), (-1, [11]), (0, [2]), (2, [3]), (3, [13]), (-1, [6]), (0, [9]), (-1, [1]), (0, [12]), (-1, [7])],
        ];

        foreach (var selections in validGames)
        {
            var provider = CreateTest("mafia-vicino-ru.json", options =>
            {
                options.TestRoles = [("Дон", 1), ("Бомж", 1), ("Маньяк", 1), ("Комиссар", 1), ("Доктор", 1), ("Мафия", 2), ("Мирный", 8)];
                options.Selections = selections;
            });
            var game = provider.GetRequiredService<Game>();

            await game.Start();
        }
    }

}
