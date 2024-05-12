namespace Mafia.Test.Model;

public class TestDebugOptions
{
    public required bool HostInstructions { get; set; }
    public required bool CitySelections { get; set; }
    public required int Seed { get; set; }
    public required (string name, int count)[] RolesPreset { get; set; }
}