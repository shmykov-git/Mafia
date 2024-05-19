namespace Mafia.Model;

public class User
{
    public required string Nick { get; set; }

    public override string ToString() => Nick;
}
