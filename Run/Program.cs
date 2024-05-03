using Mafia;
using Mafia.Interactors;
using Mafia.Services;
using Microsoft.Extensions.DependencyInjection;

var seed = 0;

var services = new ServiceCollection();

services
    .AddMafia()
    .AddTransient<IInteractor>(_ => new RunInteractor(seed));

var provider = services.BuildServiceProvider();

var game = provider.GetService<Game>();
game.Start();


var stop = 1;
