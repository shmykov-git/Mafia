using Mafia.Model;

namespace Host.Model;

public class ActivePlayer : NotifyPropertyChanged
{
    public ActivePlayer(Player player, Action<string> onChange)
    {
        Player = player;
        Subscribe(onChange);
    }

    public Player Player { get; }


    public bool IsSelectedSilent;
    public bool IsSelected { get => IsSelectedSilent; set { IsSelectedSilent = value; Changed(); } }


    public bool IsEnabledSilent;
    public bool IsEnabled { get => IsEnabledSilent; set { IsEnabledSilent = value; Changed(); } }
    
}
