using Mafia.Extensions;
using Mafia.Libraries;

namespace Mafia.Model;

public class Action
{
    public bool ByGroup { get; set; }
    public string[]? Conditions { get; set; }
    public required string[] Operations { get; set; }
    public Argument[]? Arguments { get; set; }

    public async Task<DailyNews> GetFailedActionNews(State state, Player player)
    {
        string? failedCondition = null;

        foreach (var condition in Conditions!)
            if (!(await CheckCondition(condition, state, player)))
            {
                failedCondition = condition;
                break;
            }

        if (failedCondition == null)
            throw new InvalidOperationException();

        return new DailyNews { SelectLocks = [new SelectLock { FailedCondition = failedCondition, Who = player, SkippedOperations = Operations }] };
    }

    public async Task<bool> CheckConditions(State state, Player player) => Conditions == null || await Conditions.AllAsync(name => CheckCondition(name, state, player));
    public Task<bool> CheckCondition(string name, State state, Player player) => Execution.Conditions[name](state, player);

    public async Task<DailyNews> DoOperations(State state, Player player)
    {
        var result = new DailyNews();
        foreach (var operation in Operations)
            result.Collect(await DoOperation(operation, state, player));

        return result;
    }

    public bool IsSkippable() => AllArguments().Contains(Argument.Skippable) || AllConditions().Intersect(Values.SkippableConditions).Any();

    public Task<DailyNews> DoOperation(string operation, State state, Player player) => Execution.Operations[operation](state, player, this);

    public IEnumerable<Argument> AllArguments() => Arguments ?? [];
    public IEnumerable<string> AllConditions() => Conditions ?? [];
    public required Execution Execution { get; set; }
}
