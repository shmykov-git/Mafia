using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Mafia.Model;

namespace Mafia;

public static class DependencyExtensions
{
    /// <summary>
    /// Add Mafia game
    /// Must be implemented: <see cref="IHost"/>
    /// </summary>
    public static IServiceCollection AddMafia(this IServiceCollection services, City city)
    {
        return services
            .AddTransient(_ => city)
            .AddTransient<Func<IHost>>(p => () => p.GetRequiredService<IHost>())
            .AddTransient<Game>();
    }
}
