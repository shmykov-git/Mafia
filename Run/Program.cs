using Mafia;
using Mafia.Interactors;
using Mafia.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var seed = 90;

//for (var i = 0; i < 1000; i++)

var interactor = new RunInteractor(seed);

var builder = new ConfigurationBuilder();
builder.AddJsonFile(interactor.GameFileName);
var configuration = builder.Build();

var services = new ServiceCollection();

services
    .AddMafia(configuration)
    .AddTransient<IInteractor>(_ => interactor);

var provider = services.BuildServiceProvider();

var game = provider.GetService<Game>();
game.Start();


var stop = 1;
