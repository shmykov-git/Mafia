using Host.Model;
using Mafia.Extensions;

namespace Host.Views;

public partial class UserView : ContentPage
{
    public UserView()
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
        if (e.Item is ActiveUser activeUser)
        {
            activeUser.IsSelected = !activeUser.IsSelected;
        }
    }
}
