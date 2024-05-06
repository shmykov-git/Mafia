namespace Mafia.Model;


public interface IHost
{
    Player[] GetPlayers();


    void NotifyCityAfterNight(State state);
    Player AskCityToSelect(State state);
    void NotifyCityAfterDay(State state);
    bool AskCityToSkip(State state);
    void NotifyGameEnd(State state);

    Player AskToSelect(State state, Player player);
    Player AskToSelectNotSelf(State state, Player player);
    Player[] GetNeighbors(State state, Player player);
    bool AskToSkip(State state, Player player);
}
