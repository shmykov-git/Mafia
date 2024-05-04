using System.Data;
using Mafia.Extensions;
using Mafia.Interactors;
using Mafia.Models;
using Mapster;
using Microsoft.Extensions.Options;

namespace Mafia.Services;

public class Game
{
    public Model model;
    public DateTime time;
    public readonly Player[] players;
    private IInteractor interactor;
    private readonly IOptions<GameOptions> options;
    private List<List<Event>> process = new();
    private Dictionary<string, int> allRanks;

    public List<Player> alivePlayers;

    public Game(IInteractor interactor, IOptions<GameOptions> options)
    {
        this.interactor = interactor;
        this.options = options;
        interactor.ApplyGame(this);
        LoadModel(interactor.GameFileName);
        players = interactor.GetPlayers(model!);
        alivePlayers = players.ToList();
        allRanks = GetRanks(model!.Roles);
    }

    public void Start()
    {
        // game end
        // don't blame doctor
        // players config
        time = DateTime.Now;
        alivePlayers = players.ToList();
        
        PlayDay(1, false);
        PlayDay(1, true);
        PlayDay(2, false);
        PlayDay(2, true);
        PlayDay(3, false);
        PlayDay(3, true);
        PlayDay(4, false);
        PlayDay(4, true);
    }

    private Event[] PlayAfterKill(Player[] kills)
    {
        var procKills = kills.ToList();
        List<Event> allKillEvents = new();

        while (procKills.Count > 0)
        {
            var killed = procKills[0];
            procKills.RemoveAt(0);

            var planEvents = model.OnDeathEvents.FirstOrDefault(v => v.Role == killed.Role)?.Events;

            if (planEvents == null)
                continue;

            var killEvents = planEvents.Select(Interact).Where(evt => evt != null).ToArray();
            allKillEvents.AddRange(killEvents!);

            var newKills = GetKills(killEvents!);
            procKills.AddRange(newKills);
        }

        return allKillEvents.ToArray();
    }

    private void PlayDay(int day, bool night)
    {
        bool IsMineTimeOfDay(Event e) => (e.group == null || !GetGroup(e.group).IsCity) == night;

        var time = night ? "night" : "day";
        interactor.Log($"======== <{time} {day}> ========");

        var dayPlanEvents = day == 1 ? model.FirstDayEvents : model.DayEvents;
        
        List<Event> dayEvents = new();

        foreach (var planEvent in dayPlanEvents.Where(IsMineTimeOfDay))
        {
            var locks = GetLocks(dayEvents);
            var evt = Interact(planEvent, locks);
            
            if (evt == null)
                continue;

            dayEvents.Add(evt);
        }

        var kills = GetKills(dayEvents!);
        var killEvents = PlayAfterKill(kills);

        if (killEvents.Length > 0)
        {
            dayEvents = dayEvents.Concat(killEvents).ToList();
            kills = GetKills(dayEvents!);
        }

        kills.ForEach(p => alivePlayers.Remove(p));

        interactor.Log($"Alive players: {alivePlayers.OrderBy(p => allRanks[p.Role]).SJoin(", ")}");

        process.Add(dayEvents!);

        interactor.Log($"======== </{time} {day}> =======");
    }

    HashSet<Player> GetLocks(ICollection<Event> events) => events.Where(e => e.info.IsPersonEvent && e.info.act == Act.Lock && e.selections != null).SelectMany(e => e.selections).ToHashSet();

    private static HashSet<Act> killActs = new[] { Act.Kill, Act.KillOnDeath, Act.DoubleKillOnDeath }.ToHashSet();
    Player[] GetKills(ICollection<Event> events)
    {
        var locks = GetLocks(events);
        var kills = events.Where(e => e.info.IsPersonEvent && e.info.act.HasValue && killActs.Contains(e.info.act.Value) && e.selections != null && e.info.mainPlayers!.Except(locks).Any()).SelectMany(e => e.selections).ToList();
        var heals = events.Where(e => e.info.IsPersonEvent && e.info.act == Act.Heal && e.selections != null && e.info.mainPlayers!.Except(locks).Any()).SelectMany(e => e.selections).ToArray();
        heals.ForEach(p => kills.Remove(p));
        locks.ForEach(p => kills.Remove(p));

        var cityKills = events.Where(e => e.info.IsCityEvent && e.info.act == Act.Kill && e.selections != null).SelectMany(e => e.selections).ToArray();
        kills.AddRange(cityKills);

        return kills.ToArray();
    }

    //public (string[] events, string[] kills) DoActs() { return (null, null); }

    private EventInfo GetEventInfo(Event evt)
    {
        if (evt.command == Command.Select)
        {
            if (evt.group == null)
            {
                var act = model.SelectActs.First(s => s.Role == evt.role);

                return new EventInfo { roles = [evt.role!], command = evt.command, act = act.Act, players = GetPlayersByRoles([evt.role!]), skippable = act.Skippable };
            }

            var group = model.Groups.First(g => g.Name == evt.group);

            if (group.IsCity)
                return new EventInfo { roles = null, command = evt.command, act = group.Act!.Value, players = null, skippable = group.Skippable };

            var role = group.Roles!.Select(r => model.SelectActs.First(s => s.Role == r)).GroupBy(v => v.Act).Select(v => v.First()).Single();

            return new EventInfo { roles = group.Roles, command = evt.command, act = role.Act, players = GetPlayersByRoles(group.Roles!), skippable = role.Skippable };
        }
        else
        {
            if (evt.group == null)
                return new EventInfo { roles = [evt.role!], players = GetPlayersByRoles([evt.role!]), command = evt.command };

            var group = model.Groups.First(g => g.Name == evt.group);

            return new EventInfo { roles = group.Roles, players = GetPlayersByRoles(group.Roles!), command = evt.command };
        }
    }

    public Dictionary<string, int> GetRanks(string[] roles) => roles.Select((r, i) => (r, i)).ToDictionary(v => v.r, v => v.i);

    private Event? Interact(Event planEvt) => Interact(planEvt, []);
    private Event? Interact(Event planEvt, HashSet<Player> locks)
    {
        if (planEvt.command == Command.GetInfo)
        {
            if (process.Any())
            {
                var kills = GetKills(process[^1]);
                interactor.Tells($"Kills: {kills.SJoin(", ")}");
                interactor.Tells($"Alive players: {alivePlayers.OrderBy(p => allRanks[p.Role]).SJoin(", ")}");
            }
            // day 1

            return null;
        }

        var evt = planEvt.Adapt<Event>();
        var eInfo = GetEventInfo(evt);
        evt.info = eInfo;

        var who = evt.group;
        var ranks = GetRanks((evt.group == null ? null : GetGroup(evt.group).Roles) ?? model.Roles);
        var whoLocked = "";

        if (eInfo.IsPersonEvent)
        {
            var locked = alivePlayers.Where(p => eInfo.roles!.Contains(p.Role)).OrderBy(p => ranks[p.Role]).FirstOrDefault();
            who = alivePlayers.Where(p => !locks.Contains(p) && eInfo.roles!.Contains(p.Role)).OrderBy(p => ranks[p.Role]).SJoin(", ");
            
            if (locked != null && locks.Contains(locked))
                whoLocked = locked.ToString();
        }

        var act = eInfo.act == null ? eInfo.command.ToString() : eInfo.act.ToString();

        if (!who.HasText())
        {
            if (whoLocked.HasText())
                if (eInfo.HasSelection)
                {
                    interactor.Info($"{whoLocked} is busy and cannot {act}");
                }
                else
                {
                    if (options.Value.TellWakeUp)
                        interactor.Tells($"{whoLocked} {act}");
                }

            return null;
        }

        if (eInfo.HasSelection)
        {
            evt.selections = GetSelection(eInfo);

            var sign = eInfo.act switch
            {
                Act.Kill => "-> ",
                Act.KillOnDeath => "-> ",
                Act.DoubleKillOnDeath => ">> ",
                _ => "-- "
            };

            var whom = evt.selections.Length == 0 ? "nobody" : evt.selections.SJoin(", ");

            if (whoLocked.HasText())
                interactor.Info($"{whoLocked} is busy and cannot {act}");

            interactor.Info($"{sign}{who} {act} {whom}");
        }
        else
        {
            if (options.Value.TellWakeUp)
                interactor.Tells($"{who} {act}");
        }

        return evt;
    }
    
    private Group GetGroup(string name) => model.Groups.Single(g => g.Name == name);
    //public string[] CanSelect(Player p) => players.Select(p => p.User.Id).ToArray();
    //public bool CanAct(Player p) => true;

    private void LoadModel(string fileName)
    {
        var json = File.ReadAllText(fileName);
        model = json.FromJson<Model>();
    }

    private Player[][]? GetPlayersByRoles(string[] arrangedRoles) => arrangedRoles == null ? null : arrangedRoles.Select(role => alivePlayers.Where(p => p.Role == role).ToArray()).Where(ps => ps.Length > 0).ToArray();

    private Player[] GetSelection(EventInfo evt)
    {
        if (evt.IsCityEvent)
            return interactor.CitySelect(evt.skippable);

        var player = evt.mainPlayers![0];

        var needSelfSkip = process.SelectMany(v => v)
            .Where(e => e.role == player.Role && (player.SelectAct?.SelfOnes ?? false) && e.selections != null)
            .SelectMany(e => e.selections)
            .Any(p => p == player);

        var selectablePlayers = needSelfSkip ? alivePlayers.Where(p => p != player).ToList() : alivePlayers;

        return evt.act switch
        {
            Act.DoubleKillOnDeath => interactor.DoubleKillOnDeath(player),
            _ => interactor.Select(player, selectablePlayers, evt.skippable)
        };
    }

}