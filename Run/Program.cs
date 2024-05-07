using System.Diagnostics;
using Mafia;
using Mafia.Extensions;
using Mafia.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Run;

var mafiaFileName = "mafia-drive.json";
var appsettingsFileName = "appsettings.json";

var builder = new ConfigurationBuilder();
builder.AddJsonFile(appsettingsFileName);
var configuration = builder.Build();

var json = File.ReadAllText(mafiaFileName);
var city = json.FromJson<City>();

var services = new ServiceCollection();

services
    .Configure<RunOptions>(configuration.GetSection("options"))
    .AddMafia(city)
    .AddSingleton<IHost, DebugHost>();

var provider = services.BuildServiceProvider();
var host = provider.GetService<IHost>();

for (var k = 0; k<1; k++)
{
    Debug.WriteLine($"\r\n'{city.Name}' game {k}");
    var game = provider.GetService<Game>();
    host.ChangeSeed(k);
    game.Start();
}


var stop = 1;
