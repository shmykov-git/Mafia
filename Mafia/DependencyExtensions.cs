using Microsoft.Extensions.DependencyInjection;
using Mafia.Interactors;
using Mafia.Services;
using Mafia.Models;
using Microsoft.Extensions.Configuration;

namespace Mafia;

public static class DependencyExtensions
{
    /// <summary>
    /// Add Mafia game
    /// Must be implemented: <see cref="IInteractor"/>
    /// </summary>
    public static IServiceCollection AddMafia(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .Configure<GameOptions>(configuration.GetSection("options"))
            .AddTransient<Game>();
    }
}
