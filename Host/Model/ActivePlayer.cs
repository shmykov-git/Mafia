using Mafia.Model;
using Microsoft.Maui.Controls;

namespace Host.Model;

public class ActivePlayer : NotifyPropertyChanged
{
    public ActivePlayer(Player player, Action<string> onChange, string propertyName)
    {
        Player = player;
        Subscribe(onChange, propertyName);
    }

    public Player Player { get; }

    public Color TextColorSilent { get; set; } = Colors.Black;
    public Color TextColor { get => TextColorSilent; set { TextColorSilent = value; Changed(); } }

    public string OperationSilent { get; set; } = string.Empty;
    public string Operation { get => OperationSilent; set { OperationSilent = value; Changed(); } }

    public bool IsSelectedSilent { get; set; }
    public bool IsSelected { get => IsSelectedSilent; set { IsSelectedSilent = value; Changed(); } }


    public bool IsEnabledSilent { get; set; }
    public bool IsEnabled { get => IsEnabledSilent; set { IsEnabledSilent = value; Changed(); } }
    
}
