using Host.ViewModel;
using Mafia.Extensions;

namespace Host.Views;

public partial class AppShell : Shell
{
    public AppShell()
    {
        try
        {
            InitializeComponent();
        }
        catch(FileNotFoundException e) when (!e.Message.HasText())
        {
        }
    }

    private void Shell_Navigated(object sender, ShellNavigatedEventArgs e)
    {
        if (BindingContext is HostViewModel hostViewModel)
        {
            if (hostViewModel.NavigatedCommand.CanExecute(e.Current.Location.ToString()))
                hostViewModel.NavigatedCommand.Execute(e.Current.Location.ToString());
        }        
    }
}
