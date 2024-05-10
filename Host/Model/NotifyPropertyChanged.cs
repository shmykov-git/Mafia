using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Host.Model;

public abstract class NotifyPropertyChanged : INotifyPropertyChanged
{
    private Dictionary<string, bool> silents = new Dictionary<string, bool>(); // no async exec
    private Action<string> subscribers = _ => { };

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void Subscribe(Action<string> onChange) { subscribers += onChange; }
    protected bool IsSilent(string propertyName) => silents.TryGetValue(propertyName, out bool silent) ? silent : false;
    protected void DoSilent(string propertyName, Action action)
    {
        silents[propertyName] = true;
        action();
        silents[propertyName] = false;
    }

    protected void Changed([CallerMemberName] string propertyName = "")
    {
        //Debug.WriteLine($"PropertyChanged: {propertyName}");
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        subscribers(propertyName);
    }

    protected void ChangedSilently([CallerMemberName] string propertyName = "")
    {
        silents[propertyName] = true;
        Changed(propertyName);
        silents[propertyName] = false;
    }
}
