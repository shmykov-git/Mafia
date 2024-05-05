using Mafia.Extensions;

namespace Mafia.Model;

public class City
{
    public required Group[] Groups { get; set; }

    public Group GetTopGroup(string name) => Groups.Single(g => g.Name == name);
    public Group GetGroup(string name) => AllGroups().Single(g => g.Name == name);
    public Role GetRole(string name) => AllRoles().Single(r => r.Name == name);
    public Group GetGroup(Role role) => AllGroups().Single(g => g.AllRoles().Contains(role));

    public IEnumerable<Group> AllGroups() => this.LazyIterateDeepLeft<Group>();
    public IEnumerable<Role> AllRoles() => AllGroups().SelectMany(group => group.AllRoles());
    public IEnumerable<Action> AllActions() => AllRoles().SelectMany(role => role.AllActions());
    public IEnumerable<string> AllConditions() => AllActions().SelectMany(action => action.Conditions ?? new string[0]).ToArray().Distinct();
    public IEnumerable<string> AllOperations() => AllActions().SelectMany(action => action.Operations ?? new string[0]).ToArray().Distinct();
}
