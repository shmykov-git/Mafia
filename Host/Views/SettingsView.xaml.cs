using Host.Model;

namespace Host.Views;

public partial class SettingsView : ContentPage
{
	public SettingsView()
	{
		InitializeComponent();
	}

    private void ListView_Lang_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item is ActiveLang activeLang)
        {
            activeLang.IsChecked = !activeLang.IsChecked;
        }
    }

    private void ListView_Club_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item is ActiveClub activeClub)
        {
            activeClub.IsChecked = !activeClub.IsChecked;
        }
    }

    private void ListView_Rule_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item is ActiveRule activeRule)
        {
            activeRule.IsAccepted = !activeRule.IsAccepted;
        }
    }
}