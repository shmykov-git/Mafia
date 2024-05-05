using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Mafia.Model;

namespace Mafia;

public static class DependencyExtensions
{
    /// <summary>
    /// Add Mafia game
    /// Must be implemented: <see cref="IInteractor"/>
    /// </summary>
    public static IServiceCollection AddMafia(this IServiceCollection services, IConfiguration configuration, City city)
    {
        return services
            .Configure<GameOptions>(configuration.GetSection("options"))
            .AddTransient(_ => city)
            .AddTransient<Game>();
    }
}
