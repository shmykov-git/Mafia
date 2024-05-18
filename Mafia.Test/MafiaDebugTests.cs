using Mafia.Model;
using Mafia.Test.Base;
using Mafia.Test.Model;
using Microsoft.Extensions.DependencyInjection;
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
    public async Task Debug_Drive_FullDifficult20_100()
    {
        async void SetOptions(TestDebugOptions options)
        {
            options.Shaffle = true;
            options.CitySelections = true;
            options.RolesPreset = [("Дон", 1), ("Путана", 1), ("Маньяк", 1), ("Комиссар", 1), ("Доктор", 1), ("Камикадзе", 1), ("Сапер", 1), ("Мафия", 4), ("Мирный", 9)];
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
            options.HostInstructions = true;
            options.CitySelections = true;
            options.RolesPreset = [("Дон", 1), ("Бомж", 1), ("Маньяк", 1), ("Комиссар", 1), ("Доктор", 1), ("Камикадзе", 1), ("Мафия", 2), ("Мирный", 7)];
        }

        await RunDebugGames("mafia-vicino-ru.json", 100, 0, SetOptions);
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
    public async Task Debug_Vicino_FullDifficult20_100()
    {
        async void SetOptions(TestDebugOptions options)
        {
            options.Shaffle = true;
            options.HostInstructions = false;
            options.CitySelections = true;
            options.RolesPreset = [("Дон", 1), ("Бомж", 1), ("Проститутка", 1), ("Маньяк", 1), ("Комиссар", 1), ("Сержант", 1), ("Доктор", 1), ("Камикадзе", 1), ("Шахид", 1), ("Мафия", 3), ("Мирный", 8)];
        }

        await RunDebugGames("mafia-vicino-ru.json", 100, 777, SetOptions);
    }



    private async Task RunDebugGames(string map, int n, int startSeed, Action<TestDebugOptions> setOptions)
    {
        var provider = CreateTestDebug(map, setOptions);
        var host = provider.GetRequiredService<IHost>();
        var game = provider.GetRequiredService<Game>();
        var city = provider.GetRequiredService<ICity>().City;

        for (var k = 0; k < n; k++)
        {
            var seed = k + startSeed;
            Debug.WriteLine($"\r\n'{city.Name}' game {seed}");
            host.ChangeSeed(seed);
            await game.Start();
        }
    }
}