using System.ComponentModel.Design;
using System.Data;
using Mafia.Exceptions;
using Mafia.Extensions;
using Mafia.Interactors;
using Mafia.Models;
using Microsoft.VisualBasic;

namespace Mafia.Modules;

public class Game
{
    public Model model;
    public DateTime time;
    public Player[] players;
    public IInteractor interactor;

    public List<Player> alivePlayers;
    public List<Event[]> Process = new();

    public string[] roles => model.Groups.Where(g => g.Roles != null).SelectMany(g => g.Roles!).Distinct().ToArray();

    public (string[] events, string[] kills) DoActs() { return (null, null); }

    public EventInfo GetEventInfo(Event evt)
    {
        if (evt.command == Command.Select)
        {
            if (evt.group == null)
            {
                var act = model.SelectActs.First(s => s.Role == evt.role);

                return new EventInfo { roles = [evt.role!], command = evt.command, act = act.Act, skippable = act.Skippable };
            }

            var group = model.Groups.First(g => g.Name == evt.group);

            if (group.IsCity)
                return new EventInfo { roles = group.Roles, command = evt.command, act = group.Act!.Value, skippable = group.Skippable };

            var role = group.Roles!.Select(r => model.SelectActs.First(s => s.Role == r)).GroupBy(v => v.Act).Select(v => v.First()).Single();

            return new EventInfo { roles = group.Roles, command = evt.command, act = role.Act, skippable = role.Skippable };
        }
        else
        {
            if (evt.group == null)
                return new EventInfo { roles = [evt.role!], command = evt.command };

            var group = model.Groups.First(g => g.Name == evt.group);

            return new EventInfo { roles = group.Roles, command = evt.command };
        }
    }

    public string[] CanSelect(Player p) => players.Select(p => p.User.Id).ToArray();
    public bool CanAct(Player p) => true;

    public void LoadModel(string fileName)
    {
        var json = File.ReadAllText(fileName);
        model = json.FromJson<Model>();
    }

    private Player[]? GetEventPlayers(EventInfo evt) =>
        evt.roles == null ? null : alivePlayers.Where(p => evt.roles.Contains(p.Role)).ToArray();

    private Player[] GetSelection(EventInfo evt)
    {
        if (evt.IsCityEvent && interactor.AskCityToSkip())
            return [];

        var actPlayer = GetEventPlayers(evt)!.Single();

        return evt.act switch
        {
            Act.DoubleKill => interactor.DoubleSelect(GetEventPlayers(evt)!.Single()),
            _ => interactor.Select(GetEventPlayers(evt)!.Single())
        };
    }

    private void Interact(Event evt)
    {
        var e = GetEventInfo(evt);
        var obj = e.roles == null ? evt.group : e.roles.SJoin(", ");
        var actMsg = e.act == null ? e.command.ToString() : e.act.ToString();

        if (e.HasSelection)
            evt.selections = GetSelection(e);

        interactor.TellInformation($"{obj} {actMsg}");
    }

    public void Play()
    {
        alivePlayers = players.ToList();

        PlayDay(1);
        PlayDay(2);
    }

    private void PlayDay(int day)
    {
        interactor.TellInformation($"======== <day {day}> ========");

        var dayEvents = day == 1 ? model.FirstDayEvents : model.DayEvents;

        foreach (var evt in dayEvents)
        {
            Interact(evt);
        }

        interactor.TellInformation($"======== </day {day}> =======");
    }
}