namespace Mafia.Model;

public class Rule
{
    public required RuleName Name { get; set; }
    public string? Description { get; set; }
    public bool Accepted {  get; set; }
    public string[] Values { get; set; }

    public string[] GetListValues(int i) => Values[i].Split(',').Select(v => v.Trim()).ToArray();
}
