namespace Mafia.Model;

public class Action
{
    public required string Name { get; set; }
    public string[]? Conditions { get; set; }
    public required string[] Operations { get; set; }

    public bool CheckConditions(State state, Player player) => Conditions == null || Conditions.All(name => CheckCondition(name, state, player));
    public bool CheckCondition(string name, State state, Player player) => Execution.Conditions[name](state, player);

    public OperationResult DoOperations(State state, Player player)
    {
        var result = new OperationResult();
        foreach (var name in Operations)
            result.Collect(DoOperation(name, state, player));

        return result;
    }

    public OperationResult DoOperation(string name, State state, Player player) => Execution.Operations[name](state, player);

    public IEnumerable<string> AllConditions() => Conditions ?? new string[0];
    public required Execution Execution { get; set; }
}
