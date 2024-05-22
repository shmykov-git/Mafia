namespace Mafia.Test.Model;

public class TestDebugOptions
{
    public bool ShafflePlaces { get; set; }
    public bool ShaffleRoles { get; set; }
    public bool ShowRating { get; set; }
    public required bool HostInstructions { get; set; }
    public required bool Debug { get; set; }
    public required int Seed { get; set; }
    public required (string name, int count)[] RolesPreset { get; set; }
}