
using System.Diagnostics;
using Mafia;



var nMax = 20;
var rnd = new Random();
var players = Enumerable.Range(1, nMax+1).Select(i => new User { Name = $"Player {i}", Nick = i.ToString() }).ToArray();

var listP = players.ToList();

var n = 10;
var gamePlayers = Enumerable.Range(1, n+1).Select(_ =>
{
    var i = rnd.Next(listP.Count);
    var player = listP[i];
    listP.RemoveAt(i);
    return player;
}).ToArray();


var now = DateTime.Now;
var game = new Game() { Time = now };
game.LoadModel("mafia.json");

var roles = game.roles;




var stop = 1;


//var plan = new[]
//{
//    ("Город засыпает", GameEvent.CityFallAsleep),
//    ("Просыпается мафия", GameEvent.MafiaKill),
//    ("Просыпается маньяк", GameEvent.ManiacKill),
//    ("Просыпается доктор", GameEvent.DoctorSave),
//    ("Просыпается комиссар и сержант", GameEvent.CommissarAsk),
//    ("Просыпается камикадзе", GameEvent.KamikazeHello),
//    ("Просыпается город", GameEvent.CityWakeUp),
//};


foreach (var _ in Enumerable.Range(0, rnd.Next(17)))
{
    var i = rnd.Next(roles.Length);
    var j = rnd.Next(roles.Length);
    (roles[i], roles[j]) = (roles[j], roles[i]);
}

var playerRoles = roles.Select((r, i)=> new Player { User = gamePlayers[i], Role = roles[i] }).ToList();
var lives = playerRoles.ToList();





//int SomeOne() => rnd.Next(lives.Count);

//void Kill(int day, GameEvent evt, int ind)
//{
//    // check kill event

//    var act = new GameAction() { Day = day, Event = evt };
//    game.Actions.Add(act);

//    var live = lives[ind];
//    live.Day = day;
//    lives.Remove(live);
//}

void ShowState()
{
    Debug.WriteLine("=======");
    lives.ForEach(l => Debug.WriteLine($"{l.User.Name} {l.Role}"));
}

// plan, allowed events

var day = 1;
ShowState();

// 1 day

//Kill(day, GameEvent.CityKill, SomeOne());
ShowState();

// 1 night

//var nightActions = new (GameEvent evt, int pInd)[] 
//{
//    (GameEvent.MafiaKill, SomeOne(Role.Mafia, Role.DonMafia, Role.BumMafia)), // not necessary
//    (GameEvent.ManiacKill, SomeOne(Role.Maniac)),
//    (GameEvent.DoctorSave, SomeOne()), // check ones
//};

// 2 day

void ProcessNightActions((GameEvent evt, int pInd)[] actions)
{
    
}

