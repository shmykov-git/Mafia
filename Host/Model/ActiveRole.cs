using Mafia.Model;
namespace Host.Model;

public class ActiveRole : NotifyPropertyChanged
{
    public ActiveRole(Role role, Action<string, ActiveRole> onChange, string propertyName)
    {
        Role = role;
        Subscribe((name, obj) => onChange(name, (ActiveRole)obj), propertyName);
    }

    public Role Role { get; }
    public bool IsCounter => Role.IsMultiple;
    public bool IsCheckbox => !Role.IsMultiple;


    public bool IsSelectedSilent { get; set; }
    public bool IsSelected { get => IsSelectedSilent; set { IsSelectedSilent = value; Changed(); } }


    public int CountSilent { get; set; }
    public int Count { get => CountSilent; set { CountSilent = value; Changed(); } }
}
