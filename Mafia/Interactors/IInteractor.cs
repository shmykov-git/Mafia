using Mafia.Models;

namespace Mafia.Interactors;

public interface IInteractor
{
    bool AskCityToSkip();
    Player[] Select(Player player);
    Player[] DoubleSelect(Player player);
    void TellInformation(string message);
}
