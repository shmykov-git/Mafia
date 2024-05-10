using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Host.Model;

public abstract class NotifyPropertyChanged : INotifyPropertyChanged
{
    private Dictionary<string, bool> silents = new Dictionary<string, bool>(); // no async exec
    private Action<string> subscribers = _ => { };
    private Action<string, NotifyPropertyChanged> objectSubscribers = (_, _) => { };

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void Subscribe(Action<string, NotifyPropertyChanged> onChange, string propertyName) { objectSubscribers += (name, obj) => DoSilent(propertyName, () => onChange(name, obj)); }
    protected void Subscribe(Action<string> onChange, string propertyName) { subscribers += name => DoSilent(propertyName, () => onChange(name)); }

    //protected void Subscribe(Action<string> onChange) { subscribers += onChange; }
    //protected bool IsSilent(string propertyName) => silents.TryGetValue(propertyName, out bool silent) ? silent : false;

    protected void DoSilent(string propertyName, Action onChange)
    {
        if (silents.TryGetValue(propertyName, out var value) ? value : false)
            return;

        onChange();
    }

    protected void Changed([CallerMemberName] string propertyName = "")
    {
        //Debug.WriteLine($"PropertyChanged: {propertyName}");
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        subscribers(propertyName);
        objectSubscribers(propertyName, this);
    }

    protected void ChangedSilently([CallerMemberName] string propertyName = "")
    {
        silents[propertyName] = true;
        Changed(propertyName);
        silents[propertyName] = false;
    }
}
