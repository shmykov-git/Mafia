using Mafia.Model;

namespace Host.Model;

public class ActiveUser : NotifyPropertyChanged
{
    public ActiveUser(User user, Action<string, ActiveUser> onChange, string propertyName)
    {
        User = user;
        Subscribe((name, obj) => onChange(name, (ActiveUser)obj), propertyName);
    }

    public Color NickColorSilent { get; set; } = Colors.Black;
    public Color NickColor { get => NickColorSilent; set { NickColorSilent = value; Changed(); } }

    public User User { get; }

    public string Nick { get => User.Nick; set { User.Nick = value; Changed(); } }

    public bool IsSelectedSilent;
    public bool IsSelected { get => IsSelectedSilent; set => IfChanged(ref IsSelectedSilent, value); }
    
}
