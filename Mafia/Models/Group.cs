namespace Mafia.Models;

public class Group
{
    public required string Name { get; set; }
    public string[]? Roles { get; set; }
    public Act? Act { get; set; }
    public bool Skippable { get; set; }
    public bool IsCity => Roles == null;
}
