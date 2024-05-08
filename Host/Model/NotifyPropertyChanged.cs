using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Host.Model;

public abstract class NotifyPropertyChanged : INotifyPropertyChanged
{
    private Action<string> subscribers = _ => { };

    public void Subscribe(Action<string> onChange) {  subscribers += onChange; }

    public event PropertyChangedEventHandler? PropertyChanged;
    public void Changed([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        subscribers(propertyName);
    }
}
