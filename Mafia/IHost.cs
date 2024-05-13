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
    Task<User[]> AskCityToSelect(State state, CityAction action, string operation);
    Task NotifyCityAfterDay(State state);
    Task NotifyGameEnd(State state, Group group);
    Task<User[]> AskToSelect(State state, Player player, Action action, string operation);
    Task<User[]> GetNeighbors(State state, Player player, Action action, string operation);
    Task<bool> IsGameEnd(State state);
}
