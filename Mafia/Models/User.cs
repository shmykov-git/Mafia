namespace Mafia.Models;

public class User
{
    public string Id => Nick;
    public required string Nick { get; set; }
    public required string Name { get; set; }
    public string? Phone { get; set; }
    public string? Pic { get; set; }
}
