using Mafia.Executions;
using Mafia.Extensions;
using Mafia.Model;
using Microsoft.Extensions.Options;

namespace Mafia;

public class Game 
{
    private readonly IOptions<GameOptions> options;
    private readonly City city;
    private State state;

    public Game(IOptions<GameOptions> options, City city)
    {
        this.options = options;
        this.city = city;

        InitCity();
    }

    private void InitCity()
    {
        //var operations = city.AllOperations().ToArray();
        //var conditions = city.AllConditions().ToArray();

        city.AllActions().ForEach(action =>
        {
            var conditions = action.Conditions?.Select(name => typeof(Conditions).GetMethods().First(m => m.Name == name))
                .ToDictionary(m => m.Name, m => (Condition)((s, p) => (bool)m.Invoke(null, new object[] { s, p })));

            var operations = action.Operations.Select(name => typeof(Operations).GetMethods().First(m => m.Name == name))
                .ToDictionary(m => m.Name, m => (Operation)((s, p) => (OperationResult)m.Invoke(null, new object[] { s, p })));

            action.Execution = new Execution { Conditions = conditions, Operations = operations };
        });
    }

    public void Start()
    {

    }
}
