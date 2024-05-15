namespace Mafia.Model;

public class User
{
    public required string Nick { get; set; }
    public required DateTime LastPlay {  get; set; }

    public override string ToString() => Nick;
}
