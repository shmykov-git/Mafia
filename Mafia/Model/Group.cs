﻿using Mafia.Executions;
using Mafia.Extensions;

namespace Mafia.Model;

public class Group
{
    public required string Name { get; set; }
    public Role[]? Roles { get; set; }
    public Group[]? Groups { get; set; }

    public bool HasAnyOperation(string[] operations) => AllRoles().Any(r => r.AllActions().Any(a => a.Operations.Intersect(operations).Any()));

    public bool HasRanks() => AllRoles().Select(r => r.Rank).Distinct().Count() > 1;

    public IEnumerable<Role> AllRoles() => Roles ?? [];
    public IEnumerable<Role> AllDeepRoles() => AllGroups().SelectMany(g=>g.AllRoles());
    public IEnumerable<Group> AllGroups() => this.IterateLazyDeepLeft<Group>();
    public IEnumerable<Group> AllLeafGroups() => AllGroups().Where(group => group.Groups == null);
}
