namespace Mafia.Model;


public interface IHost
{
    void ChangeSeed(int seed);
    (User user, string role)[] GetUserRoles();

    Task NotifyCityAfterNight(State state);
    Task NotifyNightStart(State state);
    Task NotifyDayStart(State state);
    Task<Player> AskCityToSelect(State state);
    Task NotifyCityAfterDay(State state);
    Task<bool> AskCityToSkip(State state);
    Task NotifyGameEnd(State state, Group group);

    Task<Player[]> AskToSelect(State state, Player player);
    Task<Player[]> GetNeighbors(State state, Player player);
    Task<bool> AskToSkip(State state, Player player);
    Task<bool> IsGameEnd(State state);
}
