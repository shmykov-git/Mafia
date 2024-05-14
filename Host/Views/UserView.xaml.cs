using Host.Model;

namespace Host.Views;

public partial class UserView : ContentPage
{
    public UserView()
    {
        InitializeComponent();        
    }

    private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item is ActiveUser activeUser)
        {
            activeUser.IsSelected = !activeUser.IsSelected;
        }
    }
}
