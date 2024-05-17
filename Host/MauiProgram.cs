using Mafia.Model;
using Mafia;
using Mafia.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using Host.Views;
using Host.ViewModel;
using Host.Model;
using System.Diagnostics;
using Microsoft.Maui.ApplicationModel;

namespace Host;

public static class MauiProgram
{
    private const string settingsFileName = "appsettings.json";
    private const string mapFolder = "Resources/Maps";

    private static City GetCity()
    {
        var builder = new ConfigurationBuilder();
        using Stream settingsStream = FileSystem.Current.OpenAppPackageFileAsync(settingsFileName).Result;
        builder.AddJsonStream(settingsStream);
        var configuration = builder.Build();
        
        var options = configuration.GetSection("options").Get<HostMapOptions>();

        var cities = options.Maps.Select(map =>
        {
            using Stream mafiaStream = FileSystem.Current.OpenAppPackageFileAsync(Path.Combine(mapFolder, map)).Result;
            using TextReader textReader = new StreamReader(mafiaStream);
            var json = textReader.ReadToEnd();
            var city = json.FromJson<City>()!;
            return city;
        }).ToArray();

        return cities.Single(c => c.Name == options.DefaultMapName && c.Language == options.DefaultLanguage);
    }


    public static MauiApp CreateMauiApp()
    {
        var city = GetCity();

        using Stream settingsStream = FileSystem.Current.OpenAppPackageFileAsync(settingsFileName).Result;

        var builder = MauiApp.CreateBuilder();
        builder.Configuration.AddJsonStream(settingsStream);

        builder.Services
            .Configure<HostOptions>(builder.Configuration.GetSection("options"))
            .AddMafia(city)
            .AddSingleton<HostViewModel>()
            .AddSingleton<IHost, HostViewModel>(p => p.GetRequiredService<HostViewModel>())
            .AddTransient(p => new AppShell() { BindingContext = p.GetRequiredService<HostViewModel>() })
            .AddTransient(p => new UserView() { BindingContext = p.GetRequiredService<HostViewModel>() })
            .AddTransient(p => new RoleView() { BindingContext = p.GetRequiredService<HostViewModel>() })
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
