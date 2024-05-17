using Host.Model;

namespace Host.Views;

public partial class RoleView : ContentPage
{
    public RoleView()
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
