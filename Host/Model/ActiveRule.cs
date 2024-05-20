using Mafia.Model;
namespace Host.Model;

public class ActiveRule : NotifyPropertyChanged
{
    public ActiveRule(Rule rule)
    {
        Rule = rule;
    }

    public Rule Rule { get; }
    public string Description => Rule.Description;

    public bool IsAccepted { get => Rule.Accepted; set { Rule.Accepted = value; Changed(); } }
}
