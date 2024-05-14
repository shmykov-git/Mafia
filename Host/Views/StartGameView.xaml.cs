using Host.Model;

namespace Host.Views;

public partial class StartGameView : ContentPage
{
    public StartGameView()
    {
        InitializeComponent();        
    }

    private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item is ActiveRole activeRole)
        {
            activeRole.IsSelected = !activeRole.IsSelected;
        }
    }
}
