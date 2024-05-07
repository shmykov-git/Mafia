﻿using Mafia.Extensions;

namespace Mafia.Model;

public class City
{
    public required string Name { get; set; }
    public required Group[] Groups { get; set; }
    public required string[] NightEvents { get; set; }
    public required CityAction[] DayActions { get; set; }
    public required Rule[] Rules { get; set; }

    public Player CreatePlayer(User user, Role role) => new Player
    {
        User = user,
        Role = role,
        Group = GetGroup(role),
        TopGroup = GetTopGroup(role),
    };

    public Rule GetRule(RuleName name) => Rules.Single(r => r.Name == name);
    public Group GetTopGroup(Role role) => Groups.Single(g => g.AllGroups().Any(gg => gg.AllRoles().Any(r => r == role)));
    public Group GetTopGroup(string name) => Groups.Single(g => g.Name == name);
    public Group GetGroup(string name) => AllGroups().Single(g => g.Name == name);
    public Role GetRole(string name) => AllRoles().Single(r => r.Name == name);
    public Role GetMultipleRole(string name) => AllRoles().Single(r => r.Name == name && r.IsMultiple);
    public Group GetGroup(Role role) => AllGroups().Single(g => g.AllRoles().Contains(role));
    public Role[] GetRoles(string[] roles) => AllRoles().Where(r => roles.Contains(r.Name)).ToArray();

    public IEnumerable<Group> AllGroups() => this.IterateLazyDeepLeft<Group>();
    public IEnumerable<Role> AllRoles() => AllGroups().SelectMany(group => group.AllRoles());
    public IEnumerable<Role> AllMultipleRoles() => AllGroups().SelectMany(group => group.AllRoles().Where(r => r.IsMultiple));
    public IEnumerable<Action> AllActions() => AllRoles().SelectMany(role => role.AllActions());
    public IEnumerable<string> AllConditions() => AllActions().SelectMany(action => action.Conditions ?? []).ToArray().Distinct();
    public IEnumerable<string> AllOperations() => AllActions().SelectMany(action => action.Operations ?? []).ToArray().Distinct();
}