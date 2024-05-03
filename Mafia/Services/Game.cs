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
    private List<Event[]> process = new();

    public List<Player> alivePlayers;

    public Game(IInteractor interactor, IOptions<GameOptions> options)
    {
        this.interactor = interactor;
        this.options = options;
        interactor.ApplyGame(this);
        LoadModel(interactor.GameFileName);
        players = interactor.GetPlayers(model!);
        alivePlayers = players.ToList();
    }

    public void Start()
    {
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
        interactor.Tells($"======== <{time} {day}> ========");

        var dayPlanEvents = day == 1 ? model.FirstDayEvents : model.DayEvents;
        var dayEvents = dayPlanEvents.Where(IsMineTimeOfDay).Select(Interact).Where(evt => evt != null).ToArray();

        var kills = GetKills(dayEvents!);
        var killEvents = PlayAfterKill(kills);

        if (killEvents.Length > 0)
        {
            dayEvents = dayEvents.Concat(killEvents).ToArray();
            kills = GetKills(dayEvents!);
        }

        kills.ForEach(p => alivePlayers.Remove(p));

        interactor.Tells($"Kills: {kills.SJoin(", ")}");
        interactor.Tells($"Alive players: {alivePlayers.OrderBy(p=>p.Group?.Name).ThenBy(p=>p.Role).SJoin(", ")}");

        interactor.Tells($"======== </{time} {day}> =======");

        process.Add(dayEvents!);
    }

    private static HashSet<Act> killActs = new[] { Act.Kill, Act.KillOnDeath, Act.DoubleKillOnDeath }.ToHashSet();
    Player[] GetKills(Event[] dayProcess)
    {
        var locks = dayProcess.Where(e => e.info.IsPersonEvent && e.info.act == Act.Lock && e.selections != null).SelectMany(e => e.selections).ToHashSet();
        var kills = dayProcess.Where(e => e.info.IsPersonEvent && e.info.act.HasValue && killActs.Contains(e.info.act.Value) && e.selections != null && e.info.mainPlayers!.Except(locks).Any()).SelectMany(e => e.selections).ToList();
        var heals = dayProcess.Where(e => e.info.IsPersonEvent && e.info.act == Act.Heal && e.selections != null && e.info.mainPlayers!.Except(locks).Any()).SelectMany(e => e.selections).ToArray();
        heals.ForEach(p => kills.Remove(p));
        locks.ForEach(p => kills.Remove(p));

        var cityKills = dayProcess.Where(e => e.info.IsCityEvent && e.info.act == Act.Kill && e.selections != null).SelectMany(e => e.selections).ToArray();
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

    private Event? Interact(Event planEvt)
    {
        var evt = planEvt.Adapt<Event>();
        var eInfo = GetEventInfo(evt);
        evt.info = eInfo;

        var who = evt.group;

        if (eInfo.IsPersonEvent)
            who = alivePlayers.Where(p => eInfo.roles!.Contains(p.Role)).SJoin(", ");

        if (!who.HasText())
            return null;

        var act = eInfo.act == null ? eInfo.command.ToString() : eInfo.act.ToString();

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

            interactor.Tells($"{sign}{who} {act} {whom}");
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

        return evt.act switch
        {
            Act.DoubleKillOnDeath => interactor.DoubleKillOnDeath(player),
            _ => interactor.Select(player, evt.skippable)
        };
    }

}