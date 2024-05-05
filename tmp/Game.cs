using System.Data;
using Mafia.Extensions;
using Mafia.Interactors;
using Mafia.Model;
using Mafia.Models;
using Mapster;
using Microsoft.Extensions.Options;
using tmp.Interactors;

namespace tmp;

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

        var day = 1;
        while (true)
        {
            Play12(day, false);

            if (process[^1][^1].command == Command.GameEnd)
                break;

            Play12(day, true);

            if (process[^1][^1].command == Command.GameEnd)
                break;

            day++;
        }
    }

    private Event[] PlayAfterKill(Player[] kills, bool night)
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

            List<Event> killEvents = new();

            foreach (var planEvent in planEvents)
            {
                var evt = Interact(planEvent, [], night);

                if (evt == null)
                    continue;

                killEvents.Add(evt);
            }

            allKillEvents.AddRange(killEvents!);

            var newKills = GetKills(killEvents!);
            procKills.AddRange(newKills);
        }

        return allKillEvents.ToArray();
    }

    private ICollection<Event> ApplyKills(ICollection<Event> events, bool night)
    {
        var kills = GetKills(events);
        var killEvents = PlayAfterKill(kills, night);

        if (killEvents.Length > 0)
        {
            events = events.Concat(killEvents).ToList();
            kills = GetKills(events);
        }

        kills.ForEach(p => alivePlayers.Remove(p));

        return events;
    }

    private void Play12(int day, bool night)
    {
        bool IsMineTimeOfDay(Event e) => (e.group == null || !GetGroup(e.group).IsCity) == night;
        var isFirstDay = day == 1;

        var time = night ? "night" : "day";
        interactor.Log($"======== <{time} {day}> ========");

        List<Event> events = new();

        foreach (var planEvent in model.Events.Where(IsMineTimeOfDay))
        {
            if (planEvent.firstDay && !isFirstDay)
                continue;

            var evt = Interact(planEvent, events, night);

            if (evt == null)
                continue;

            events.Add(evt);

            if (evt.command == Command.GameEnd)
                break;
        }

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

    private void TellAlivePlayers()
    {
        interactor.Tells($"Alive players: {alivePlayers.OrderBy(p => allRanks[p.Role]).SJoin(", ")}");
    }

    private void TellState(ICollection<Event> events)
    {
        var kills = GetKills(events);
        interactor.Tells($"Kills: {kills.SJoin(", ")}");
        TellAlivePlayers();
    }

    private Event? Interact(Event planEvt, ICollection<Event> events, bool night)
    {
        if (!night && (planEvt.command == Command.WakeUp || planEvt.command == Command.FallAsleep))
            return null;

        var locks = GetLocks(events);

        if (planEvt.command == Command.ApplyKills)
        {
            events = ApplyKills(events, night);
            var gameEnd = GetGameEnd();

            Event? endGameEvent = null;

            if (gameEnd != GameEnd.None)
            {
                endGameEvent = new Event() { command = Command.GameEnd };
                events.Add(endGameEvent);

                TellAlivePlayers();
                interactor.Tells($"GameEnd: {gameEnd}");
            }

            process.Add(events.ToList());

            return endGameEvent;
        }

        if (planEvt.command == Command.GetMorningInfo)
        {
            if (process.Any()) // day 1 ?
                TellState(process[^1]);

            return null;
        }

        if (planEvt.command == Command.GetDayInfo)
        {
            TellState(events);

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

    private GameEnd GetGameEnd()
    {
        var counts = alivePlayers.GroupBy(p => p.ParticipantGroup).ToDictionary(gv => gv.Key.End, gv => gv.Count());

        if (counts.Keys.Count == 1)
            return counts.Keys.First();

        var mafia = counts.TryGetValue(GameEnd.Mafia, out var countMafia) ? countMafia : 0;
        var maniac = counts.TryGetValue(GameEnd.Maniac, out var countManiac) ? countManiac : 0;
        var civilian = counts.TryGetValue(GameEnd.Civilian, out var countCivilian) ? countCivilian : 0;

        if (maniac == 0)
        {
            if (mafia >= civilian)
                return GameEnd.Mafia;
        }
        else
        {
            if (mafia == 0)
            {
                if (civilian <= 1)
                    return GameEnd.Maniac;
            }
        }

        return GameEnd.None;
    }
}