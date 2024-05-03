using Mafia.Models;
using Mafia.Services;

namespace Mafia.Interactors;

public interface IInteractor
{
    void ApplyGame(Game game);
    Player[] GetPlayers(Model model);

    string GameFileName { get; }

    Player[] CitySelect(bool skippable);
    Player[] Select(Player player, bool skippable);
    Player[] DoubleKillOnDeath(Player player);
    void Tells(string message);
}
