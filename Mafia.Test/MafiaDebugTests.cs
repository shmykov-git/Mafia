using Mafia.Model;
using Mafia.Test.Base;
using Mafia.Test.Model;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Mafia.Test;

public class MafiaDebugTests : MafiaTestsBase
{
    [Fact]
    public async Task Debug_Classic12_100()
    {
        var n = 100;

        void SetOptions(TestDebugOptions options)
        {
            options.Seed = 0;
            options.HostInstructions = false;
            options.CitySelections = true;
            options.RolesPreset = [("Дон", 1), ("Бомж", 1), ("Маньяк", 1), ("Комиссар", 1), ("Доктор", 1), ("Мафия", 1), ("Мирный", 6)];
        }

        var provider = CreateTestDebug("mafia-vicino-ru.json", SetOptions);
        var host = provider.GetRequiredService<IHost>();
        var game = provider.GetRequiredService<Game>();
        var city = provider.GetRequiredService<City>();

        for (var k = 0; k < n; k++)
        {
            Debug.WriteLine($"\r\n'{city.Name}' game {k}");
            host.ChangeSeed(k);
            await game.Start();
        }
    }

    [Fact]
    public async Task Debug_Classic15_100()
    {
        var n = 100;

        void SetOptions(TestDebugOptions options)
        {
            options.Seed = 0;
            options.HostInstructions = false;
            options.CitySelections = true;
            options.RolesPreset = [("Дон", 1), ("Бомж", 1), ("Маньяк", 1), ("Комиссар", 1), ("Доктор", 1), ("Камикадзе", 1), ("Мафия", 2), ("Мирный", 7)];
        }

        var provider = CreateTestDebug("mafia-vicino-ru.json", SetOptions);
        var host = provider.GetRequiredService<IHost>();
        var game = provider.GetRequiredService<Game>();
        var city = provider.GetRequiredService<City>();

        for (var k = 0; k < n; k++)
        {
            Debug.WriteLine($"\r\n'{city.Name}' game {k}");
            host.ChangeSeed(k);
            await game.Start();
        }
    }

    [Fact]
    public async Task Debug_Difficult15_100()
    {
        var n = 100;

        void SetOptions(TestDebugOptions options)
        {
            options.Seed = 0;
            options.HostInstructions = false;
            options.CitySelections = true;
            options.RolesPreset = [("Дон", 1), ("Бомж", 1), ("Проститутка", 1), ("Маньяк", 1), ("Комиссар", 1), ("Доктор", 1), ("Мафия", 2), ("Мирный", 7)];
        }

        var provider = CreateTestDebug("mafia-vicino-ru.json", SetOptions);
        var host = provider.GetRequiredService<IHost>();
        var game = provider.GetRequiredService<Game>();
        var city = provider.GetRequiredService<City>();

        for (var k = 0; k < n; k++)
        {
            Debug.WriteLine($"\r\n'{city.Name}' game {k}");
            host.ChangeSeed(k);
            await game.Start();
        }
    }
}