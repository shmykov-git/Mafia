using Host.Model;
using Host.Views;
using Microsoft.Extensions.Options;
using Mafia.Extensions;

namespace Host;

public partial class App : Application
{
    private readonly HostOptions options;

    public App(IOptions<HostOptions> options, AppShell shell)
    {
        try
        {
            InitializeComponent();
        }
        catch (FileNotFoundException e) when (!e.Message.HasText())
        {
        }
        MainPage = shell;
        this.options = options.Value;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);

        if (options.AdjustWindow)
        {
            window.X = options.WindowRect.X;
            window.Y = options.WindowRect.Y;
            window.Width = options.WindowRect.Width;
            window.Height = options.WindowRect.Height;
        }

        return window;
    }
}
