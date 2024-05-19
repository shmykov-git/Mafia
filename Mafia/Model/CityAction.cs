using Mafia.Extensions;
using Mafia.Libraries;

namespace Mafia.Model;

public class CityAction
{
    public string[]? Conditions { get; set; }
    public required string[] Operations { get; set; }
    public Argument[]? Arguments { get; set; }

    public async Task<bool> CheckConditions(State state) => Conditions == null || await Conditions.AllAsync(name => CheckCondition(name, state));
    public Task<bool> CheckCondition(string name, State state) => Execution.Conditions[name](state);

    public async Task<DailyNews> DoOperations(State state)
    {
        var result = new DailyNews();

        foreach (var operation in Operations)
            result.Collect(await DoOperation(operation, state));

        return result;
    }
    public bool IsSkippable() => AllArguments().Contains(Argument.Skippable) || AllConditions().Intersect(Values.SkippableConditions).Any();

    public Task<DailyNews> DoOperation(string operation, State state) => Execution.Operations[operation](state, this);
    
    public IEnumerable<Argument> AllArguments() => Arguments ?? [];
    public IEnumerable<string> AllConditions() => Conditions ?? [];
    public required CityExecution Execution { get; set; }
}
