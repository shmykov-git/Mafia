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
    .AddTransient<IHost, DebugHost>();

var provider = services.BuildServiceProvider();

var game = provider.GetService<Game>();
game.Start();



var stop = 1;
