using Mafia.Model;
using Mafia.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mafia.Test.Model;
using Mafia.Test.Hosts;

namespace Mafia.Test.Base;

public class MafiaTestsBase
{

    protected IServiceProvider CreateReplayTest(string mapFileName, Replay replay)
    {
        var mafiaFileName = $"Maps/{mapFileName}";

        var json = File.ReadAllText(mafiaFileName);
        var city = json.FromJson<City>();

        var services = new ServiceCollection();

        void Configure(TestDebugReplayOptions options)
        {
            options.Replay = replay;
            replay.MapName = city.Name;
            replay.MapVersion = city.Version;
            replay.Language = city.Language;
        }

        services
            .Configure<TestDebugReplayOptions>(Configure)
            .AddMafia(city)
            .AddSingleton<IHost, TestDebugReplayHost>()
            ;

        return services.BuildServiceProvider();
    }

    protected IServiceProvider CreateTestDebug(string mapFileName, Action<TestDebugOptions> configureOptions)
    {
        var mafiaFileName = $"Maps/{mapFileName}";

        var json = File.ReadAllText(mafiaFileName);
        var city = json.FromJson<City>();

        var services = new ServiceCollection();

        services
            .Configure<TestDebugOptions>(configureOptions)
            .AddMafia(city)
            .AddSingleton<IHost, TestDebugHost>();

        return services.BuildServiceProvider();
    }
}
