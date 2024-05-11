using System;

namespace Mafia.Model;


public interface IHost
{
    void ChangeSeed(int seed);
    string[] GetGameRoles();
    Task StartGame(State state);

    Task NotifyCityAfterNight(State state);
    Task NotifyNightStart(State state);
    Task NotifyDayStart(State state);
    Task<Player[]> AskCityToSelect(State state, CityAction action);
    Task NotifyCityAfterDay(State state);
    Task NotifyGameEnd(State state, Group group);
    Task<Player[]> AskToSelect(State state, Player player, Action action);
    Task<Player[]> GetNeighbors(State state, Player player, Action action);
    Task<bool> IsGameEnd(State state);
}
