namespace Host.Model;

public class ActivePlayerFilter : NotifyPropertyChanged
{
    public ActivePlayerFilter(Action onChange)
    {
        Subscribe(onChange);
    }

    public bool IsAliveSilent { get; set; }
    public bool Killed { get => IsAliveSilent; set { IsAliveSilent = value; Changed(); } }
}
