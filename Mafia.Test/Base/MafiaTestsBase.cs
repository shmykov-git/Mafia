using Mafia.Model;
using Mafia.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mafia.Test.Model;
using Mafia.Test.Hosts;

namespace Mafia.Test.Base;

public class MafiaTestsBase
{

    protected IServiceProvider CreateTest(string mapFileName, Action<TestOptions> configureOptions)
    {
        var mafiaFileName = $"Maps/{mapFileName}";

        var json = File.ReadAllText(mafiaFileName);
        var city = json.FromJson<City>();

        var services = new ServiceCollection();

        services
            .Configure<TestOptions>(configureOptions)
            .AddMafia(city)
            .AddSingleton<IHost, TestHost>();

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
