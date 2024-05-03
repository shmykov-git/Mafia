
using System.Diagnostics;
using Mafia;
using Mafia.Models;
using Mafia.Modules;



var nMax = 20;
var rnd = new Random();
var users = Enumerable.Range(1, nMax+1).Select(i => new User { Name = $"User {i}", Nick = $"Nick{i}" }).ToArray();

var listP = users.ToList();

var n = 10;
var gamePlayers = Enumerable.Range(1, n+1).Select(_ =>
{
    var i = rnd.Next(listP.Count);
    var player = listP[i];
    listP.RemoveAt(i);
    return player;
}).ToArray();


var now = DateTime.Now;
var game = new Game() { time = now };
game.LoadModel("mafia.json");

var roles = game.roles;


foreach (var _ in Enumerable.Range(0, rnd.Next(17)))
{
    var i = rnd.Next(roles.Length);
    var j = rnd.Next(roles.Length);
    (roles[i], roles[j]) = (roles[j], roles[i]);
}

var players = roles.Select((r, i)=> new Player { User = gamePlayers[i], Role = roles[i] }).ToArray();
game.players = players;

game.Play();

var stop = 1;
