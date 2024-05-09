using Mafia.Model;
using Microsoft.Maui.Controls;

namespace Host.Model;

public class ActivePlayer : NotifyPropertyChanged
{
    public ActivePlayer(Player player, Action<string> onChange)
    {
        Player = player;
        Subscribe(onChange);
    }

    public Player Player { get; }

    public Color TextColorSilent = Colors.Black;
    public Color TextColor { get => TextColorSilent; set { TextColorSilent = value; Changed(); } }

    public string OperationSilent = string.Empty;
    public string Operation { get => OperationSilent; set { OperationSilent = value; Changed(); } }

    //public FontAttributes FontAttributesSilent = FontAttributes.None;
    //public FontAttributes FontAttributes { get => FontAttributesSilent; set { FontAttributesSilent = value; Changed(); } }

    public bool IsSelectedSilent;
    public bool IsSelected { get => IsSelectedSilent; set { IsSelectedSilent = value; Changed(); } }


    public bool IsEnabledSilent;
    public bool IsEnabled { get => IsEnabledSilent; set { IsEnabledSilent = value; Changed(); } }
    
}
