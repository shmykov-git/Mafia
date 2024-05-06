using Mafia.Extensions;

namespace Mafia.Model;

public class Group
{
    public required string Name { get; set; }
    public Role[]? Roles { get; set; }
    public Group[]? Groups { get; set; }

    public IEnumerable<Role> AllRoles() => Roles ?? [];
    public IEnumerable<Group> AllGroups() => this.IterateLazyDeepLeft<Group>();
    public IEnumerable<Group> AllLeafGroups() => AllGroups().Where(group => group.Groups == null);
}
