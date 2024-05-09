using Mafia.Extensions;
using Mafia.Libraries;

namespace Mafia.Model;

public class Action
{
    public required string Name { get; set; }
    public string[]? Conditions { get; set; }
    public required string[] Operations { get; set; }

    public async Task<bool> CheckConditions(State state, Player player) => Conditions == null || await Conditions.AllAsync(name => CheckCondition(name, state, player));
    public Task<bool> CheckCondition(string name, State state, Player player) => Execution.Conditions[name](state, player);

    public async Task<DailyNews> DoOperations(State state, Player player)
    {
        var result = new DailyNews();
        foreach (var name in Operations)
            result.Collect(await DoOperation(name, state, player));

        return result;
    }

    public bool IsSkippable() => AllConditions().Intersect(Values.SkippableConditions).Any();

    public Task<DailyNews> DoOperation(string name, State state, Player player) => Execution.Operations[name](state, player, this);

    public IEnumerable<string> AllConditions() => Conditions ?? [];
    public required Execution Execution { get; set; }
}
