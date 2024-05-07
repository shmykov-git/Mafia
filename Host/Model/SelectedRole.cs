using System.ComponentModel;
using Mafia.Model;
using Action = System.Action;

namespace Host.Model;

public class SelectedRole : INotifyPropertyChanged
{
    private readonly Action<string> onChange;

    public SelectedRole(Role role, Action<string> refresh)
    {
        Role = role;
        this.onChange = refresh;
    }

    public Role Role { get; }
    public bool IsCounter => Role.IsMultiple;
    public bool IsCheckbox => !Role.IsMultiple;



    private bool _isSelected;
    public bool IsSelected { get => _isSelected; set { _isSelected = value; Changed(nameof(IsSelected)); } }

    private int _count;
    public int Count { get => _count; set { _count = value; Changed(nameof(Count)); } }



    public event PropertyChangedEventHandler? PropertyChanged;
    public void Changed(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        onChange(propertyName);
    }
}
