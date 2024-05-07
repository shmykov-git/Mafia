using Mafia.Model;
using Mafia;
using Mafia.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using Host.Views;
using Host.ViewModel;
using Host.Model;

namespace Host;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var mafiaFileName = "Resources/Mafia/mafia-drive.json";
        var appsettingsFileName = "appsettings.json";

        using Stream fileStream = FileSystem.Current.OpenAppPackageFileAsync(mafiaFileName).Result;
        using TextReader tr = new StreamReader(fileStream);
        var json = tr.ReadToEnd();

        var city = json.FromJson<City>();

        using Stream settingsStream = FileSystem.Current.OpenAppPackageFileAsync(appsettingsFileName).Result;
        var builder = MauiApp.CreateBuilder();
        builder.Configuration.AddJsonStream(settingsStream);
        //builder.Configuration.AddJsonFile(appsettingsFileName);

        builder.Services
            .Configure<HostOptions>(builder.Configuration.GetSection("options"))
            .AddMafia(city)
            .AddSingleton<HostViewModel>()
            .AddSingleton<IHost, HostViewModel>(p => p.GetRequiredService<HostViewModel>())
            .AddTransient<StartGameView>(p => new StartGameView() { BindingContext = p.GetRequiredService<HostViewModel>() })
            .AddTransient<GameView>(p => new GameView() { BindingContext = p.GetRequiredService<HostViewModel>() })
            ;

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
		builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
