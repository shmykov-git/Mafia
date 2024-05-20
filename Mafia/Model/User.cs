namespace Mafia.Model;

public class User
{
    public required string Nick { get; set; }

    public bool IsSelected { get; set; } = true;

    public override string ToString() => Nick;
}
