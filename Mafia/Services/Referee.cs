using Mafia.Extensions;
using Mafia.Hosts;
using Mafia.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Mafia.Services;

public class Rating
{
    public bool IsSupported { get; set; }
    public required PlayerRating[] PlayerRatings { get; set; }
}

public class PlayerRating
{
    public required string Nick { get; set; }
    public required string Role { get; set; }
    public required RatingCase[] Cases { get; set; }
    public int Rating => Cases.Select(GetBonus).Sum();

    private int GetBonus(RatingCase ratingCase) => ratingCase switch
    {
        RatingCase.Winner => 3,
        RatingCase.Loser => 1,
        RatingCase.Alive => 2,
        RatingCase.HealMaster => 3,
        RatingCase.KillMaster => 3,
        _ => throw new NotImplementedException()
    };
}

public class Referee
{
    public async Task<Rating> GetRating(Replay replay, City city)
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
                return new Rating { PlayerRatings = [], IsSupported = false };
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

        return new Rating { PlayerRatings = rating.GetRatings(), IsSupported = true };
    } 
}
