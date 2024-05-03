using Microsoft.Extensions.DependencyInjection;
using Mafia.Interactors;
using Mafia.Services;

namespace Mafia;

public static class DependencyExtensions
{
    /// <summary>
    /// Add Mafia game
    /// Must be implemented: <see cref="IInteractor"/>
    /// </summary>
    public static IServiceCollection AddMafia(this IServiceCollection services)
    {
        return services.AddTransient<Game>();
    }
}
