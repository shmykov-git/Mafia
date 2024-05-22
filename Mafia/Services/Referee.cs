using Mafia.Extensions;
using Mafia.Hosts;
using Mafia.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Mafia.Services;

public class Referee
{
    public async Task<(string nick, int rating, RatingCase[] cases)[]> GetRatings(Replay replay, City city)
    {
        var services = new ServiceCollection();

        switch (replay.MapName) 
        {
            case "Mafia Vicino":
                services.AddSingleton<IHost, VicinoRatingHost>();
                break;

            case "Mafia Vicino (maniac party)":
                services.AddSingleton<IHost, VicinoManiacPartyRatingHost>();
                break;

            default:
                throw new NotImplementedException(replay.MapName);
        }

        services
            .AddMafia(city)
            .AddTransient(p => (IRating)p.GetRequiredService<IHost>())
            .AddTransient(_ => replay)
            ;

        var provider = services.BuildServiceProvider();

        var game = provider.GetRequiredService<Game>();
        await game.Start();

        var rating = provider.GetRequiredService<IRating>();

        return rating.GetRatings();
    } 
}
