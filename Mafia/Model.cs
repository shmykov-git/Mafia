using Mafia.Extensions;

namespace Mafia;

public class Group
{
    public required string Name { get; set; }
    public string[]? Roles { get; set; }
}

public class RoleAct
{
    public required string Role { get; set; }
    public required Act Act {  get; set; }
}

public class Model
{
    public required Group[] Groups { get; set; }
    public required RoleAct[] Roles {  get; set; }

    public required Evt[] FirstDayEvents { get; set; }
    public required Evt[] DayEvents { get; set; }
}

public class User
{
    public string Id => Nick;
    public required string Nick { get; set; }
    public required string Name { get; set; }
    public string? Phone { get; set; }
    public string? Pic { get; set; }
}

public class Player
{
    public User User { get; set; }
    public string Role { get; set; }
    public int? Day { get; set; }
}

//public enum Role
//{
//    Civilian,
//    DonMafia,
//    BumMafia,
//    Mafia,
//    Commissar,
//    Sergeant,
//    Doctor,
//    Maniac,
//    Prostitute,
//    Kamikaze,
//    Shahid
//}

public enum SimpleAct
{
    CityKill,
    WakeUp,
    Select,
    FallAsleep
}

public enum Act
{
    CityKill,
    Kill,
    DoubleKill,
    Heal,
    Take,
    DoubleTake,
    Ask,
    Lock,
    WakeUp,
    FallAsleep
}

public class Evt
{
    public string? group;
    public string? role;
    public required SimpleAct act;
}

//public class Selection
//{
//    public Evt evt;
//    public string selected;
//}

public class MaestroMessage
{
    public string[] roles;
    public Act act;
    public string message;
}

public class GameEvent
{
    //public string description;
    public bool necessary = true;
    public Evt[] events;
    public string[] selections;
}

//public static class Values
//{
//    public static MaestroMessage[] Messages = new[]
//    {
//        new MaestroMessage{ roles = new Role[0], act = Act.WakeUp, message = "Просыпается просыпается" },
//        new MaestroMessage{ roles = [Role.Prostitute], act = Act.WakeUp, message = "Просыпается проститутка" },
//        new MaestroMessage{ roles = [Role.BumMafia, Role.DonMafia, Role.Mafia], act = Act.WakeUp, message = "Просыпается мафия в полном составе, дон, бомж и мафия" },
//        new MaestroMessage{ roles = [Role.DonMafia, Role.Mafia], act = Act.WakeUp, message = "Просыпается дон и мафия" },
//        new MaestroMessage{ roles = [Role.BumMafia, Role.Mafia], act = Act.WakeUp, message = "Просыпается мафия и бомж" },
//        new MaestroMessage{ roles = [Role.Mafia], act = Act.WakeUp, message = "Просыпается оставшаяся мафия" },
//    };

//    public static Act[] SelectionActs => [Act.CityKill, Act.Kill, Act.Save, Act.Ask, Act.Lock];
//    private static Evt[] NightEvt(Role[] rs, Act act) => [(rs, Act.WakeUp), (rs, act), (rs, Act.FallAsleep)];
//    private static Evt[] NightHelloEvt(Role[] rs) => [(rs, Act.WakeUp), (rs, Act.FallAsleep)];


//    public static GameEvent CityKill => new GameEvent { events = [([], Act.CityKill)], necessary = false };
//    public static GameEvent Prostitute => new GameEvent { events = NightEvt([Role.Prostitute], Act.Lock) };
//    public static GameEvent MafiaKill => new GameEvent { description = "Просыпается мафия", events = NightEvt([Role.DonMafia, Role.BumMafia, Role.Mafia], Act.Kill) };
//    public static GameEvent ManiacKill => new GameEvent { description = "Просыпается маньяк", events = NightEvt([Role.Maniac], Act.Kill) };
//    public static GameEvent DoctorSave => new GameEvent { description = "Просыпается доктор", events = NightEvt([Role.Doctor], Act.Save) };
//    public static GameEvent CommissarAsk => new GameEvent { description = "Просыпается комиссар и сержант", events = NightEvt([Role.Commissar, Role.Sergeant], Act.Ask) };
//    public static GameEvent KamikazeHello => new GameEvent { description = "Просыпается камикадзе познакомиться", events = NightHelloEvt([Role.Kamikaze])};
//    public static GameEvent KamikazeKill => new GameEvent { description = "Просыпается камикадзе", events = NightEvt([Role.Kamikaze], Act.Kill) };
//    public static GameEvent ShahidHello => new GameEvent { description = "Просыпается шахид познакомиться", events = NightHelloEvt([Role.Shahid]) };
//    public static GameEvent ShahidKill => new GameEvent { description = "Просыпается шахид", events = NightEvt([Role.Shahid], Act.DoubleKill) };


//    public static GameEvent[] FirstDayPlan => [CityKill, Prostitute, MafiaKill, DoctorSave, CommissarAsk, KamikazeHello, ShahidHello];
//    public static GameEvent[] DayPlan => [CityKill, Prostitute, MafiaKill, DoctorSave, CommissarAsk];
//    public static Dictionary<Role, GameEvent> KillPlan => new() { [Role.Kamikaze] = KamikazeKill, [Role.Shahid] = ShahidKill };
//}

public partial class Game
{
    public Model model;

    public string[] roles => model.Groups.Where(g=>g.Roles != null).SelectMany(g=>g.Roles!).Distinct().ToArray();

    public DateTime Time { get; set; }
    public List<Player> Players { get; set; }
    
    public List<List<GameEvent>> Process = new();

    public string[] CanSelect(Player p) => Players.Select(p=>p.User.Id).ToArray();
    public bool CanAct(Player p) => true;

    public void LoadModel(string fileName)
    {
        var json  = File.ReadAllText(fileName);
        model = json.FromJson<Model>();
    }
}