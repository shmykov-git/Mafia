using Mafia.Model;
using Mafia.Models;

namespace tmp.Interactors;

public interface IInteractor
{
    void ApplyGame(Game game);
    Player[] GetPlayers(Model model);

    string GameFileName { get; }

    Player[] CitySelect(bool skippable);
    Player[] Select(Player player, ICollection<Player> players, bool skippable);
    Player[] DoubleKillOnDeath(Player player);

    void Info(string message);
    void Tells(string message);
    void Log(string message);
}
