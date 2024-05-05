using Mafia;
using Mafia.Extensions;
using Mafia.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Run;

var seed = 0;
var mafiaFileName = "mafia.json";

var builder = new ConfigurationBuilder();
builder.AddJsonFile(mafiaFileName);
var configuration = builder.Build();

var json = File.ReadAllText(mafiaFileName);
var model = json.FromJson<Model>();
var host = new DebugHost(seed, model.City);

var services = new ServiceCollection();

services
    .AddMafia(configuration, model.City)
    .AddTransient<IHost>(_ => host);

var provider = services.BuildServiceProvider();

var game = provider.GetService<Game>();
game.Start();



var stop = 1;
