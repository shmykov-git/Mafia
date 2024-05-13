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
        var mafiaFileName = "Resources/Maps/mafia-vicino-ru.json";
        var settingsFileName = "appsettings.json";

        using Stream mafiaStream = FileSystem.Current.OpenAppPackageFileAsync(mafiaFileName).Result;
        using Stream settingsStream = FileSystem.Current.OpenAppPackageFileAsync(settingsFileName).Result;

        using TextReader textReader = new StreamReader(mafiaStream);
        var json = textReader.ReadToEnd();
        var city = json.FromJson<City>();

        var builder = MauiApp.CreateBuilder();
        builder.Configuration.AddJsonStream(settingsStream);

        builder.Services
            .Configure<HostOptions>(builder.Configuration.GetSection("options"))
            .AddMafia(city)
            .AddSingleton<HostViewModel>()
            .AddSingleton<IHost, HostViewModel>(p => p.GetRequiredService<HostViewModel>())
            .AddTransient(p => new AppShell() { BindingContext = p.GetRequiredService<HostViewModel>() })
            .AddTransient(p => new UserView() { BindingContext = p.GetRequiredService<HostViewModel>() })
            .AddTransient(p => new StartGameView() { BindingContext = p.GetRequiredService<HostViewModel>() })
            .AddTransient(p => new GameView() { BindingContext = p.GetRequiredService<HostViewModel>() })
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
