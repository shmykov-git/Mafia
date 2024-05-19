namespace Mafia.Model;

public class RolePreset
{
    public required string[] SingleRoles { get; set; }
    public required string[] MultipleRoles { get; set; }
    public required double[] Ratios { get; set; }
}