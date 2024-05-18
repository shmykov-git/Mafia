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
            options.RolesPreset = [("���", 1), ("������", 1), ("��������", 1), ("������", 1), ("�����", 2), ("������", 6)];
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
            options.RolesPreset = [("���", 1), ("������", 1), ("������", 1), ("��������", 1), ("������", 1), ("���������", 1), ("�����", 1), ("�����", 4), ("������", 9)];
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
            options.RolesPreset = [("���", 1), ("����", 1), ("������", 1), ("��������", 1), ("������", 1), ("�����", 1), ("������", 6)];
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
            options.RolesPreset = [("���", 1), ("����", 1), ("������", 1), ("��������", 1), ("������", 1), ("���������", 1), ("�����", 2), ("������", 7)];
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
            options.RolesPreset = [("���", 1), ("����", 1), ("�����������", 1), ("������", 1), ("��������", 1), ("������", 1), ("�����", 2), ("������", 7)];
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
            options.RolesPreset = [("���", 1), ("����", 1), ("�����������", 1), ("������", 1), ("��������", 1), ("�������", 1), ("������", 1), ("���������", 1), ("�����", 1), ("�����", 3), ("������", 8)];
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