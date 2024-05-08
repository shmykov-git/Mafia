using System.ComponentModel;
using System.Runtime.CompilerServices;
using Mafia.Model;
namespace Host.Model;

public class Player0 : NotifyPropertyChanged
{
    public Player0(Role role, Action<string> onChange)
    {
        Role = role;
        Subscribe(onChange);
    }

    public Role Role { get; }
    public bool IsCounter => Role.IsMultiple;
    public bool IsCheckbox => !Role.IsMultiple;


    private bool _isSelected;
    public bool IsSelected { get => _isSelected; set { _isSelected = value; Changed(); } }

    private int _count;
    public int Count { get => _count; set { _count = value; Changed(); } }
}
