﻿using Mafia.Model;

namespace Host.Model;

public class Interaction
{
    public required string Name { get; set; }
    public string? SubName { get; set; }
    public string[]? Args { get; set; }
    public Role[] WakeupRoles { get; set; } = [];
    public required State State { get; set; }
    public Player? Player { get; set; }

    public Func<User[]>? GetExceptFn { get; set; }
    public Func<User[]>? GetUnwantedFn { get; set; }
    public User[] Except { get; set; } = [];
    public User[] Unwanted { get; set; } = [];
    public Player[] Killed { get; set; } = [];
    public HostTail[] Tails { get; set; } = [];
    public (int from, int to) Selection { get; set; } = (0, 0);
    public string? Operation { get; set; }
    public bool SkipRoleSelection { get; set; }

    public bool NeedFirstDayWakeup => State.DayNumber == 1 && WithPlayer;
    public bool NeedSelection => Selection != (0, 0);
    public bool IsSkippable => Selection.from == 0;
    public bool WithCity => Player == null;
    public bool WithPlayer => Player != null;
}
