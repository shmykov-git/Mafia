using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Mafia.Model;
using Mafia.Services;

namespace Mafia;

public static class DependencyExtensions
{
    /// <summary>
    /// Add Mafia game
    /// Must be implemented: <see cref="IHost"/>, <see cref="ICity"/>
    /// </summary>
    public static IServiceCollection AddMafia(this IServiceCollection services, City? city = null)
    {
        if (city != null)
            services.AddTransient<ICity, DefaultCityFactory>(p => new DefaultCityFactory(city));

        return services
            .AddTransient<Referee>()
            .AddTransient<Func<ICity>>(p => () => p.GetRequiredService<ICity>())
            .AddTransient<Func<IHost>>(p => () => p.GetRequiredService<IHost>())
            .AddTransient<Game>();
    }
}
