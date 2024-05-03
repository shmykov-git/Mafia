using System.ComponentModel.Design;
using System.Data;
using Mafia.Exceptions;
using Mafia.Extensions;
using Mafia.Interactors;
using Mafia.Models;
using Microsoft.VisualBasic;

namespace Mafia.Services;

public class Game
{
    public Model model;
    public DateTime time;
    public readonly Player[] players;
    private IInteractor interactor;

    public List<Player> alivePlayers;
    public List<Event[]> Process = new();

    public Game(IInteractor interactor)
    {
        this.interactor = interactor;
        interactor.ApplyGame(this);
        LoadModel(interactor.ModelFileName);
        players = interactor.GetPlayers(model!);
        alivePlayers = players.ToList();
    }

    public void Start()
    {
        time = DateTime.Now;
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

    //public (string[] events, string[] kills) DoActs() { return (null, null); }

    private EventInfo GetEventInfo(Event evt)
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

    //public string[] CanSelect(Player p) => players.Select(p => p.User.Id).ToArray();
    //public bool CanAct(Player p) => true;

    private void LoadModel(string fileName)
    {
        var json = File.ReadAllText(fileName);
        model = json.FromJson<Model>();
    }

    private Player GetPlayerByRole(string role) => players.Single(p => p.Role == role);
    private Player GetActPlayer(EventInfo evt) => alivePlayers.First(p => evt.roles!.Contains(p.Role));

    private Player[] GetSelection(EventInfo evt)
    {
        if (evt.IsCityEvent)
            return interactor.CitySelect(evt.skippable);

        var actPlayer = GetActPlayer(evt);

        return evt.act switch
        {
            Act.DoubleKill => interactor.DoubleSelect(actPlayer),
            _ => interactor.Select(actPlayer, evt.skippable, evt.act!.Value)
        };
    }

    private void Interact(Event evt)
    {
        var e = GetEventInfo(evt);
        var who = e.roles == null ? evt.group : e.roles.Select(r => $"{GetPlayerByRole(r).User.Nick}-{r}").SJoin(", ");
        var act = e.act == null ? e.command.ToString() : e.act.ToString();

        if (e.HasSelection)
        {
            evt.selections = GetSelection(e);

            if (evt.selections.Length == 0)
            {
                interactor.TellInformation($"{who} {act} nobody"); // todo: mafia.json
            }
            else
            {
                var players = evt.selections.Select(s => s.User.Nick).SJoin(", ");
                interactor.TellInformation($"{who} {act} {players}");

                //evt.selections.ForEach(p => alivePlayers.Remove(p));
            }
        }
        else
            interactor.TellInformation($"{who} {act}");
    }
}