namespace Mafia.Model;


public interface IHost
{
    void ChangeSeed(int seed);
    (User user, string role)[] GetUserRoles();

    void NotifyCityAfterNight(State state);
    void NotifyNightStart(State state);
    void NotifyDayStart(State state);
    Player AskCityToSelect(State state);
    void NotifyCityAfterDay(State state);
    bool AskCityToSkip(State state);
    void NotifyGameEnd(State state, Group group);

    Player AskToSelect(State state, Player player);
    Player AskToSelectNotSelf(State state, Player player);
    Player[] GetNeighbors(State state, Player player);
    bool AskToSkip(State state, Player player);
    bool IsGameEnd(State state);
}
