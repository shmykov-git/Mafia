using Mafia.Executions;
using Mafia.Extensions;
using Mafia.Model;

namespace Mafia;

public class Game 
{
    private readonly City city;
    private readonly IHost host;
    private State state;

    public Game(City city, IHost host)
    {
        this.city = city;
        this.host = host;
        InitCity();
    }

    private void InitCity()
    {
        city.DayActions.ForEach(cityAction =>
        {
            var conditions = cityAction.Conditions?.Select(name => typeof(CityConditions).GetMethods().First(m => m.Name == name))
                .ToDictionary(m => m.Name, m => (CityCondition)((s) => (bool)m.Invoke(null, new object[] { s })));

            var operations = cityAction.Operations.Select(name => typeof(CityOperations).GetMethods().First(m => m.Name == name))
                .ToDictionary(m => m.Name, m => (CityOperation)((s) => (OperationResult)m.Invoke(null, new object[] { s })));

            cityAction.Execution = new CityExecution { Conditions = conditions, Operations = operations };
        });

        city.AllActions().ForEach(action =>
        {
            var conditions = action.Conditions?.Select(name => typeof(Conditions).GetMethods().First(m => m.Name == name))
                .ToDictionary(m => m.Name, m => (Condition)((s, p) => (bool)m.Invoke(null, new object[] { s, p })));

            var operations = action.Operations.Select(name => typeof(Operations).GetMethods().First(m => m.Name == name))
                .ToDictionary(m => m.Name, m => (Operation)((s, p) => (OperationResult)m.Invoke(null, new object[] { s, p })));

            action.Execution = new Execution { Conditions = conditions, Operations = operations };
        });
    }

    private void PlayDay()
    {
        var result = new OperationResult();
        state.Processes.Add(result);

        foreach (var action in city.DayActions)
        {
            if (action.CheckConditions(state))
                result.Collect(action.DoOperations(state));
        }
    }

    private void PlayNight()
    {
        var result = new OperationResult();
        state.Processes.Add(result);

        foreach (var group in city.NightEvents.Select(groupName => city.GetGroup(groupName)))
        {
            foreach (var player in state.GetGroupPlayers(group))
            {
                foreach (var action in player.Role.AllActions())
                {
                    if (action.CheckConditions(state, player))
                        result.Collect(action.DoOperations(state, player));
                }
            }
        }
    }

    public void Start()
    {
        var players0 = host.GetPlayers();
        
        state = new State 
        { 
            Host = host, 
            City = city,
            Players0 = players0, 
            Players = players0.ToList(), 
            IsDay = true, 
            DayNumber = 1, 
            Processes = new() 
        };

        state.IsDay = true;
        host.NotifyCityAfterNight(state);
        PlayDay();
        host.NotifyCityAfterDay(state);
        state.IsDay = false;
        PlayNight();
    }
}
