using Mafia.Model;

namespace Host.Model;

public class ActiveUser : NotifyPropertyChanged
{
    public ActiveUser(User user, Action<string> onChange, string propertyName)
    {
        User = user;
        Subscribe(onChange, propertyName);
    }

    public User User { get; }

    public string Nick { get => User.Nick; set { User.Nick = value; Changed(); } }

    public bool IsSelectedSilent { get; set; }
    public bool IsSelected { get => IsSelectedSilent; set { IsSelectedSilent = value; Changed(); } }


}
