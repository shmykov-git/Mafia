namespace Mafia.Model;

public class RolePreset
{
    public required string[] SingleRoles { get; set; }
    public required string MafiaRole { get; set; }
    public required string CivilianRole { get; set; }
    public double Ratio { get; set; }
}