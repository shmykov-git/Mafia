using Mafia.Model;
using Mafia;
using Mafia.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Host.Mafia;
using System.IO;
using Host.Mafia.ViewModel;

namespace Host;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var mafiaFileName = "Resources/Mafia/mafia-vicino.json";
        var appsettingsFileName = "appsettings.json";

        using Stream fileStream = FileSystem.Current.OpenAppPackageFileAsync(mafiaFileName).Result;
        using TextReader tr = new StreamReader(fileStream);
        var json = tr.ReadToEnd();

        var city = json.FromJson<City>();

        var builder = MauiApp.CreateBuilder();
        builder.Configuration.AddJsonFile(appsettingsFileName);

        builder.Services
            .Configure<HostOptions>(builder.Configuration.GetSection("options"))
            .AddMafia(city)
            .AddSingleton<HostViewModel>()
            .AddSingleton<IHost, HostViewModel>(p => p.GetRequiredService<HostViewModel>())
            .AddTransient<MainPage>(p => new MainPage() { BindingContext = p.GetRequiredService<HostViewModel>() })
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
