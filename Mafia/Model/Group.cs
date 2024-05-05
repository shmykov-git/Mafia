namespace Mafia.Model;

public class Group
{
    public required string Name { get; set; }
    public Role[]? Roles { get; set; }
    public Group[]? Groups { get; set; }

    public IEnumerable<Role> AllRoles() => Roles ?? new Role[0];

}
