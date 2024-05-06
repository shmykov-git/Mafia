﻿namespace Mafia.Model;

public class CityAction
{
    public string[]? Conditions { get; set; }
    public required string[] Operations { get; set; }

    public bool CheckConditions(State state) => Conditions == null || Conditions.All(name => CheckCondition(name, state));
    public bool CheckCondition(string name, State state) => Execution.Conditions[name](state);

    public OperationResult DoOperations(State state)
    {
        var result = new OperationResult();
        foreach (var name in Operations)
            result.Collect(DoOperation(name, state));

        return result;
    }

    public OperationResult DoOperation(string name, State state) => Execution.Operations[name](state);


    public required CityExecution Execution { get; set; }
}
