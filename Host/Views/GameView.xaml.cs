using Host.Model;

namespace Host.Views;

public partial class GameView : ContentPage
{
    public GameView()
    {
        InitializeComponent();
    }

    private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item is ActivePlayer activePlayer)
        {
            if (activePlayer.IsEnabled)
                activePlayer.IsSelected = !activePlayer.IsSelected;
        }
    }
}
