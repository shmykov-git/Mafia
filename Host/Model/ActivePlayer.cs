using Mafia.Model;
using Microsoft.Maui.Controls;

namespace Host.Model;

public class ActivePlayer : NotifyPropertyChanged
{
    private readonly Dictionary<string, string> messages;

    public ActivePlayer(Dictionary<string, string> messages, User user, Action<string> onChange, string propertyName)
    {
        Subscribe(onChange, propertyName);
        User = user;
        this.messages = messages;
    }

    public void NotifyUserApplied()
    {
        Changed(nameof(Player));
        Changed(nameof(Nick));
    }

    public User User { get; }
    public Player? Player { get; set; }

    public string Nick => User.Nick;// ?? messages["Unknown"];
    public string Role => Player?.Role.Name ?? messages["Unknown"];

    public Color NickColorSilent { get; set; } = Colors.Black;
    public Color NickColor { get => NickColorSilent; set { NickColorSilent = value; Changed(); } }

    public Color RoleColorSilent { get; set; } = Colors.Black;
    public Color RoleColor { get => RoleColorSilent; set { RoleColorSilent = value; Changed(); } }

    public string OperationSilent { get; set; } = string.Empty;
    public string Operation { get => OperationSilent; set { OperationSilent = value; Changed(); } }

    public bool IsSelectedSilent { get; set; }
    public bool IsSelected { get => IsSelectedSilent; set { IsSelectedSilent = value; Changed(); } }

    public bool IsEnabledSilent { get; set; }
    public bool IsEnabled { get => IsEnabledSilent; set { IsEnabledSilent = value; Changed(); } }
    
}
