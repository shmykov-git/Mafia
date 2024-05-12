namespace Mafia.Test.Model;

public class TestOptions
{
    public required (string name, int count)[] TestRoles { get; set; }
    public required (int, int[])[] Selections { get; set; }
}
