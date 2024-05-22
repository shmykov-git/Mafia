using Mafia.Model;
using Mafia.Services;
using Mafia.Test.Base;
using Mafia.Test.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Mafia.Test;

public class MafiaDebugTests : MafiaTestsBase
{
    [Fact]
    public async Task Debug_Drive_Classic12_100()
    {
        void SetOptions(TestDebugOptions options)
        {
            options.CitySelections = true;
            options.RolesPreset = [("Дон", 1), ("Маньяк", 1), ("Комиссар", 1), ("Доктор", 1), ("Мафия", 2), ("Мирный", 6)];
        }

        await RunDebugGames("mafia-drive-ru.json", 100, 0, SetOptions);
    }


    [Fact]
    public async Task Debug_Drive_ru_FullDifficult20_100()
    {
        async void SetOptions(TestDebugOptions options)
        {
            options.ShafflePlaces = true;
            options.CitySelections = true;
            options.RolesPreset = [("Дон", 1), ("Путана", 1), ("Маньяк", 1), ("Комиссар", 1), ("Доктор", 1), ("Камикадзе", 1), ("Сапер", 1), ("Мафия", 4), ("Мирный", 9)];
        }

        await RunDebugGames("mafia-drive-ru.json", 100, 777, SetOptions);
    }

    public async Task Debug_Drive_en_FullDifficult20_100()
    {
        async void SetOptions(TestDebugOptions options)
        {
            options.ShafflePlaces = true;
            options.CitySelections = true;
            options.RolesPreset = [("Don", 1), ("Путана", 1), ("Маньяк", 1), ("Комиссар", 1), ("Доктор", 1), ("Камикадзе", 1), ("Сапер", 1), ("Мафия", 4), ("Мирный", 9)];
        }

        await RunDebugGames("mafia-drive-ru.json", 100, 777, SetOptions);
    }

    [Fact]
    public async Task Debug_Vicino_Classic12_100()
    {
        void SetOptions(TestDebugOptions options)
        {
            options.HostInstructions = false;
            options.CitySelections = true;
            options.RolesPreset = [("Дон", 1), ("Бомж", 1), ("Маньяк", 1), ("Комиссар", 1), ("Доктор", 1), ("Мафия", 1), ("Мирный", 6)];
        }

        await RunDebugGames("mafia-vicino-ru.json", 100, 0, SetOptions);
    }

    [Fact]
    public async Task Debug_Vicino_Classic15_100()
    {
        void SetOptions(TestDebugOptions options)
        {
            options.ShowRating = true;
            options.CitySelections = true;
            options.RolesPreset = [("Дон", 1), ("Бомж", 1), ("Маньяк", 1), ("Комиссар", 1), ("Доктор", 1), ("Камикадзе", 1), ("Мафия", 2), ("Мирный", 7)];
        }

        await RunDebugGames("mafia-vicino-ru.json", 100, 0, SetOptions);
    }

    [Fact]
    public async Task Debug_Vicino_Classic15_En_100()
    {
        void SetOptions(TestDebugOptions options)
        {
            options.ShowRating = true;
            options.ShaffleRoles = true;
            options.CitySelections = true;
            options.RolesPreset = [("Don", 1), ("Bum", 1), ("Maniac", 1), ("Commissar", 1), ("Doctor", 1), ("Kamikaze", 1), ("Mafia", 2), ("Civilian", 7)];
        }

        await RunDebugGames("mafia-vicino.json", 100, 0, SetOptions);
    }

    [Fact]
    public async Task Debug_Vicino_Difficult15_100()
    {
        void SetOptions(TestDebugOptions options)
        {
            options.HostInstructions = false;
            options.CitySelections = true;
            options.RolesPreset = [("Дон", 1), ("Бомж", 1), ("Проститутка", 1), ("Маньяк", 1), ("Комиссар", 1), ("Доктор", 1), ("Мафия", 2), ("Мирный", 7)];
        }

        await RunDebugGames("mafia-vicino-ru.json", 100, 0, SetOptions);
    }


    [Fact]
    public async Task Debug_Vicino_ru_FullDifficult20_100()
    {
        async void SetOptions(TestDebugOptions options)
        {
            options.ShafflePlaces = true;
            options.HostInstructions = false;
            options.CitySelections = true;
            options.RolesPreset = [("Дон", 1), ("Бомж", 1), ("Проститутка", 1), ("Маньяк", 1), ("Комиссар", 1), ("Сержант", 1), ("Доктор", 1), ("Камикадзе", 1), ("Шахид", 1), ("Мафия", 3), ("Мирный", 8)];
        }

        await RunDebugGames("mafia-vicino-ru.json", 100, 777, SetOptions);
    }

    [Fact]
    public async Task Debug_Vicino_FullDifficult20_100()
    {
        async void SetOptions(TestDebugOptions options)
        {
            options.ShafflePlaces = true;
            options.HostInstructions = false;
            options.CitySelections = true;
            options.RolesPreset = [("Don", 1), ("Bum", 1), ("Prostitute", 1), ("Maniac", 1), ("Commissar", 1), ("Sergeant", 1), ("Doctor", 1), ("Kamikaze", 1), ("Shahid", 1), ("Mafia", 3), ("Civilian", 8)];
        }

        await RunDebugGames("mafia-vicino.json", 100, 777, SetOptions);
    }

    [Fact]
    public async Task Debug_Vicino_ManiacParty_ru_FullDifficult15_100()
    {
        async void SetOptions(TestDebugOptions options)
        {
            options.CitySelections = true;
            options.RolesPreset = [("Дон", 1), ("Бомж", 1), ("Маньяк", 1), ("Фанат", 1), ("Комиссар", 1), ("Доктор", 1), ("Мирный", 9)];
        }

        await RunDebugGames("mafia-vicino-maniac-party-ru.json", 100, 0, SetOptions);
    }

    [Fact]
    public async Task Debug_Vicino_ManiacParty_ru_FullDifficult20_100()
    {
        async void SetOptions(TestDebugOptions options)
        {
            options.ShafflePlaces = true;
            options.CitySelections = true;
            options.RolesPreset = [("Дон", 1), ("Бомж", 1), ("Проститутка", 1), ("Маньяк", 1), ("Фанат", 2), ("Комиссар", 1), ("Сержант", 1), ("Доктор", 1), ("Камикадзе", 1), ("Шахид", 1), ("Мафия", 1), ("Мирный", 8)];
        }

        await RunDebugGames("mafia-vicino-maniac-party-ru.json", 100, 777, SetOptions);
    }


    private async Task RunDebugGames(string map, int n, int startSeed, Action<TestDebugOptions> setOptions)
    {
        var provider = CreateDebugTest(map, setOptions);
        var host = provider.GetRequiredService<IHost>();
        var game = provider.GetRequiredService<Game>();
        var city = provider.GetRequiredService<ICity>().City;
        var referee = provider.GetRequiredService<Referee>();
        var options = provider.GetRequiredService<IOptions<TestDebugOptions>>().Value;

        List<(string nick, int rating)[]> games = new();

        for (var k = 0; k < n; k++)
        {
            var seed = k + startSeed;
            Debug.WriteLine($"\r\n'{city.Name}' game {seed}");
            host.ChangeSeed(seed);
            await game.Start();

            if (options.ShowRating)
            {
                Debug.WriteLine($"~~~~~~~~~~");
                var ratings = await referee.GetRatings(game.State.Replay, city);
                games.Add(ratings);

                foreach (var p in game.State.Players0)
                {
                    var rating = ratings.Single(r => r.nick == p.User.Nick).rating;
                    Debug.WriteLine($"{p.User.Nick} ({p.Role.Name}): {rating}");
                }
            }
        }

        if (options.ShowRating)
        {
            Debug.WriteLine($"~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

            foreach (var nick in games.SelectMany(g=>g).Select(g=>g.nick).Distinct())
            {
                var nGame = games.Where(g=>g.Any(n=>n.nick == nick)).Count();
                var rating = games.Select(g => g.Single(u => u.nick == nick).rating).Sum();
                var ratingAvg = games.Select(g => g.Single(u => u.nick == nick).rating).Average();
                Debug.WriteLine($"{nick} {nGame} {rating,3} {ratingAvg:F2}");
            }
        }
    }
}