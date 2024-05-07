using System.Diagnostics;
using Mafia;
using Mafia.Extensions;
using Mafia.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Run;

var mafiaFileName = "mafia.json";

var builder = new ConfigurationBuilder();
builder.AddJsonFile(mafiaFileName);
var configuration = builder.Build();

var json = File.ReadAllText(mafiaFileName);
var model = json.FromJson<Model>();

var services = new ServiceCollection();

services
    .Configure<RunOptions>(configuration.GetSection("options"))
    .AddMafia(model.City)
    .AddSingleton<IHost, DebugHost>();

var provider = services.BuildServiceProvider();
var host = provider.GetService<IHost>();

for (var k = 0; k<1; k++)
{
    Debug.WriteLine($"\r\nGame {k}");
    var game = provider.GetService<Game>();
    host.ChangeSeed(k);
    game.Start();
}



var stop = 1;
