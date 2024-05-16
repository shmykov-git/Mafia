using Host.Model;
using Mafia.Extensions;

namespace Host.Views;

public partial class StartGameView : ContentPage
{
    public StartGameView()
    {
        try
        {
            InitializeComponent();
        }
        catch (FileNotFoundException e) when (!e.Message.HasText())
        {
        }
    }

    private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item is ActiveRole activeRole)
        {
            activeRole.IsSelected = !activeRole.IsSelected;
        }
    }
}
