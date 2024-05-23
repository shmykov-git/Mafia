using Mafia.Model;
using Mafia;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Host.Views;
using Host.ViewModel;
using Host.Model;
using Host.Libraries;
using Host.Permission;

namespace Host;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        using Stream settingsStream = FileSystem.Current.OpenAppPackageFileAsync(HostValues.SettingsFileName).Result;

        var builder = MauiApp.CreateBuilder();
        builder.Configuration.AddJsonStream(settingsStream);

        builder.Services
            .Configure<HostOptions>(builder.Configuration.GetSection("options"))
            .AddMafia()
            .AddSingleton<HostViewModel>()
            .AddTransient<PermissionFather>()
            .AddSingleton<ICity, HostViewModel>(p => p.GetRequiredService<HostViewModel>())
            .AddSingleton<IHost, HostViewModel>(p => p.GetRequiredService<HostViewModel>())
            .AddTransient(p => new AppShell() { BindingContext = p.GetRequiredService<HostViewModel>() })
            .AddTransient(p => new UserView() { BindingContext = p.GetRequiredService<HostViewModel>() })
            .AddTransient(p => new RoleView() { BindingContext = p.GetRequiredService<HostViewModel>() })
            .AddTransient(p => new GameView() { BindingContext = p.GetRequiredService<HostViewModel>() })
            .AddTransient(p => new RatingView() { BindingContext = p.GetRequiredService<HostViewModel>() })
            .AddTransient(p => new SettingsView() { BindingContext = p.GetRequiredService<HostViewModel>() })
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
