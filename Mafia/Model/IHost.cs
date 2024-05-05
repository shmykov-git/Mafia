namespace Mafia.Model;


public interface IHost
{
    Player[] GetPlayers();

    Player AskToSelect(State state, Player player);
    bool AskToSkip(State state, Player player);
}
