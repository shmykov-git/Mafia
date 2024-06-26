using Mafia.Extensions;
using Mafia.Model;
using Mafia.Services;
using Mafia.Test.Base;
using Mafia.Test.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Data;
using System.Diagnostics;
using System.Xml.Linq;

namespace Mafia.Test;

public class MafiaDebugTests : MafiaTestsBase
{
    [Fact]
    public async Task Debug_Drive_Classic12_100()
    {
        void SetOptions(TestDebugOptions options)
        {
            options.Debug = true;
            options.RolesPreset = [("���", 1), ("������", 1), ("��������", 1), ("������", 1), ("�����", 2), ("������", 6)];
        }

        await RunDebugGames("mafia-drive-ru.json", 100, 0, SetOptions);
    }


    [Fact]
    public async Task Debug_Drive_ru_FullDifficult20_100()
    {
        async void SetOptions(TestDebugOptions options)
        {
            options.ShafflePlaces = true;
            options.Debug = true;
            options.RolesPreset = [("���", 1), ("������", 1), ("������", 1), ("��������", 1), ("������", 1), ("���������", 1), ("�����", 1), ("�����", 4), ("������", 9)];
        }

        await RunDebugGames("mafia-drive-ru.json", 100, 777, SetOptions);
    }

    public async Task Debug_Drive_en_FullDifficult20_100()
    {
        async void SetOptions(TestDebugOptions options)
        {
            options.ShafflePlaces = true;
            options.Debug = true;
            options.RolesPreset = [("Don", 1), ("������", 1), ("������", 1), ("��������", 1), ("������", 1), ("���������", 1), ("�����", 1), ("�����", 4), ("������", 9)];
        }

        await RunDebugGames("mafia-drive-ru.json", 100, 777, SetOptions);
    }

    [Fact]
    public async Task Debug_Vicino_Classic12_100()
    {
        void SetOptions(TestDebugOptions options)
        {
            options.HostInstructions = false;
            options.Debug = true;
            options.RolesPreset = [("���", 1), ("����", 1), ("������", 1), ("��������", 1), ("������", 1), ("�����", 1), ("������", 6)];
        }

        await RunDebugGames("mafia-vicino-ru.json", 100, 0, SetOptions);
    }

    [Fact]
    public async Task Debug_Vicino_Classic15_100()
    {
        void SetOptions(TestDebugOptions options)
        {
            options.ShowRating = true;
            options.Debug = true;
            options.RolesPreset = [("���", 1), ("����", 1), ("������", 1), ("��������", 1), ("������", 1), ("���������", 1), ("�����", 2), ("������", 7)];
        }

        await RunDebugGames("mafia-vicino-ru.json", 100, 0, SetOptions);
    }

    [Fact]
    public async Task Debug_Vicino_Classic15_En_100()
    {
        void SetOptions(TestDebugOptions options)
        {
            options.ShowRating = true;
            options.ShaffleRoles = true;
            options.Debug = false;
            options.RolesPreset = [("Don", 1), ("Bum", 1), ("Maniac", 1), ("Commissar", 1), ("Doctor", 1), ("Kamikaze", 1), ("Mafia", 2), ("Civilian", 7)];
        }

        await RunDebugGames("mafia-vicino.json", 100, 0, SetOptions);
    }

    [Fact]
    public async Task Debug_Vicino_Difficult15_100()
    {
        void SetOptions(TestDebugOptions options)
        {
            options.HostInstructions = false;
            options.Debug = true;
            options.RolesPreset = [("���", 1), ("����", 1), ("�����������", 1), ("������", 1), ("��������", 1), ("������", 1), ("�����", 2), ("������", 7)];
        }

        await RunDebugGames("mafia-vicino-ru.json", 100, 0, SetOptions);
    }


    [Fact]
    public async Task Debug_Vicino_ru_FullDifficult20_100()
    {
        async void SetOptions(TestDebugOptions options)
        {
            options.ShafflePlaces = true;
            options.HostInstructions = false;
            options.Debug = true;
            options.RolesPreset = [("���", 1), ("����", 1), ("�����������", 1), ("������", 1), ("��������", 1), ("�������", 1), ("������", 1), ("���������", 1), ("�����", 1), ("�����", 3), ("������", 8)];
        }

        await RunDebugGames("mafia-vicino-ru.json", 100, 777, SetOptions);
    }

    [Fact]
    public async Task Debug_Vicino_FullDifficult20_100()
    {
        async void SetOptions(TestDebugOptions options)
        {
            options.ShafflePlaces = true;
            options.HostInstructions = false;
            options.Debug = true;
            options.RolesPreset = [("Don", 1), ("Bum", 1), ("Prostitute", 1), ("Maniac", 1), ("Commissar", 1), ("Sergeant", 1), ("Doctor", 1), ("Kamikaze", 1), ("Shahid", 1), ("Mafia", 3), ("Civilian", 8)];
        }

        await RunDebugGames("mafia-vicino.json", 100, 777, SetOptions);
    }

    [Fact]
    public async Task Debug_Vicino_ManiacParty_ru_Classic15_100()
    {
        async void SetOptions(TestDebugOptions options)
        {
            options.ShowRating = true;
            options.ShaffleRoles = true;
            options.Debug = false;
            options.RolesPreset = [("���", 1), ("����", 1), ("������", 1), ("�����", 1), ("��������", 1), ("������", 1), ("���������", 1), ("������", 8)];
        }

        await RunDebugGames("mafia-vicino-maniac-party-ru.json", 100, 0, SetOptions);
    }

    [Fact]
    public async Task Debug_Vicino_ManiacParty_ru_FullDifficult20_100()
    {
        async void SetOptions(TestDebugOptions options)
        {
            options.ShafflePlaces = true;
            options.Debug = true;
            options.RolesPreset = [("���", 1), ("����", 1), ("�����������", 1), ("������", 1), ("�����", 2), ("��������", 1), ("�������", 1), ("������", 1), ("���������", 1), ("�����", 1), ("�����", 1), ("������", 8)];
        }

        await RunDebugGames("mafia-vicino-maniac-party-ru.json", 100, 777, SetOptions);
    }


    private async Task RunDebugGames(string map, int n, int startSeed, Action<TestDebugOptions> setOptions)
    {
        var provider = CreateDebugTest(map, setOptions);
        var host = provider.GetRequiredService<IHost>();
        var game = provider.GetRequiredService<Game>();
        var city = provider.GetRequiredService<ICity>().City;
        var referee = provider.GetRequiredService<Referee>();
        var options = provider.GetRequiredService<IOptions<TestDebugOptions>>().Value;

        List<(string nick, string role, int rating, RatingCase[] cases)[]> gameRatings = new();

        for (var k = 0; k < n; k++)
        {
            var seed = k + startSeed;
            Debug.WriteLine($"\r\n'{city.Name}' game {seed}");
            host.ChangeSeed(seed);
            await game.Start();

            var rating = await referee.GetRating(game.State.Replay, city);

            if (rating.IsSupported && options.ShowRating)
            {
                gameRatings.Add(rating.PlayerRatings.Select(r => (r.Nick, r.Role, r.Rating, r.Cases)).ToArray());

                foreach (var r in rating.PlayerRatings)
                    Debug.WriteLine($"{r.Nick} ({r.Role}): {r.Rating} [{r.Cases.SJoin(", ")}]");
            }
        }

        if (gameRatings.Any() && options.ShowRating)
        {
            Debug.WriteLine($"~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

            var nicks = gameRatings.SelectMany(g => g).Select(g => g.nick).Distinct().ToArray();
            var roles = gameRatings.SelectMany(g => g).Select(g => g.role).Distinct().ToArray();

            foreach (var nick in nicks)
            {
                var nGame = gameRatings.Where(g=>g.Any(n=>n.nick == nick)).Count();
                var rating = gameRatings.Select(g => g.Single(u => u.nick == nick).rating).Sum();
                var ratingAvg = gameRatings.Select(g => g.Single(u => u.nick == nick).rating).Average();
                Debug.WriteLine($"{nick} {nGame} {rating,3} {ratingAvg:F2}");
            }

            var rolesRating = roles.Select(role => (role, rating: gameRatings.SelectMany(g => g.Where(u => u.role == role).Select(v => v.rating)).Average()))
                .Select(v => $"{v.role}={v.rating:F2}").SJoin(", ");

            Debug.WriteLine($"games count: {gameRatings.Count}");
            Debug.WriteLine($"average rating: {gameRatings.SelectMany(g => g).Select(g => g.rating).Average():F2}");
            Debug.WriteLine($"roles rating: {rolesRating}");
        }
    }
}