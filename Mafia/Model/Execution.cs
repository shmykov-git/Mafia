using Mafia.Executions;

namespace Mafia.Model;

public class Execution
{
    public required Dictionary<string, Condition> Conditions { get; set; }
    public required Dictionary<string, Operation> Operations { get; set; }
}