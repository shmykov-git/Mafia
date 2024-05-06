using Mafia.Executions;

namespace Mafia.Model;

public class CityExecution
{
    public required Dictionary<string, CityCondition> Conditions { get; set; }
    public required Dictionary<string, CityOperation> Operations { get; set; }
}
