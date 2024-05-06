using Mafia.Model;
using Mafia;
using Mafia.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Host.Mafia;
using System.IO;

namespace Host;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var mafiaFileName = "Resources/Mafia/mafia.json";

        using Stream fileStream = FileSystem.Current.OpenAppPackageFileAsync(mafiaFileName).Result;
        using TextReader tr = new StreamReader(fileStream);
        var json = tr.ReadToEnd();

        var model = json.FromJson<Model>();

        var builder = MauiApp.CreateBuilder();
        builder.Configuration.AddJsonFile(mafiaFileName);

        builder.Services
            .Configure<RunOptions>(builder.Configuration.GetSection("options"))
            .AddMafia(model.City)
            .AddSingleton<MainPage>()
            .AddTransient<Func<ITextBuilder>>(p=>()=>p.GetRequiredService<MainPage>())
            .AddSingleton<IHost, TextHost>();

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
