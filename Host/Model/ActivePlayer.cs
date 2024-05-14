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
    public Player? PlayerSilent { get; set; }
    public Player? Player { get => PlayerSilent; set { PlayerSilent = value; Changed(); Changed(nameof(RoleName)); } }

    public string Nick => User.Nick;
    public string RoleName => Player?.Role.Name ?? messages["Unknown"];

    public Color NickColorSilent { get; set; } = Colors.Black;
    public Color NickColor { get => NickColorSilent; set { NickColorSilent = value; Changed(); } }

    public Color RoleColorSilent { get; set; } = Colors.Black;
    public Color RoleColor { get => RoleColorSilent; set { RoleColorSilent = value; Changed(); } }

    public string OperationSilent { get; set; } = string.Empty;
    public string Operation { get => OperationSilent; set { OperationSilent = value; Changed(); } }
    
    public Color OperationColorSilent { get; set; } = Colors.Black;
    public Color OperationColor { get => OperationColorSilent; set { OperationColorSilent = value; Changed(); } }

    public Color CheckboxColorSilent { get; set; } = Colors.Black;
    public Color CheckboxColor { get => CheckboxColorSilent; set { CheckboxColorSilent = value; Changed(); } }

    public bool IsSelectedSilent { get; set; }
    public bool IsSelected { get => IsSelectedSilent; set { IsSelectedSilent = value; Changed(); } }

    public bool IsEnabledSilent { get; set; }
    public bool IsEnabled { get => IsEnabledSilent; set { IsEnabledSilent = value; Changed(); } }

    public bool IsAliveSilent { get; set; } = true;
    public bool IsAlive { get => IsAliveSilent; set { IsAliveSilent = value; Changed(); } }

    public bool IsKilled => !IsAlive;
}
