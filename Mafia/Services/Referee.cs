using Mafia.Extensions;
using Mafia.Hosts;
using Mafia.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Mafia.Services;

public class Referee
{
    public async Task<(string nick, int rating)[]> GetRatings(Replay replay, City city)
    {
        // todo: reload city

        var services = new ServiceCollection();

        services
            .AddMafia(city)
            .AddTransient(p=>(IRating)p.GetRequiredService<IHost>())
            .AddTransient(_=>replay)
            ;

        switch (replay.MapName) 
        {
            case "Mafia Vicino":
                services.AddSingleton<IHost, VicinoRatingHost>(); 
                break;

            default:
                throw new NotImplementedException(replay.MapName);
        }

        var provider = services.BuildServiceProvider();

        var game = provider.GetRequiredService<Game>();
        await game.Start();

        var rating = provider.GetRequiredService<IRating>();

        return rating.GetRatings();
    } 
}
