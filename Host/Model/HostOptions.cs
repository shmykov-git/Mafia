namespace Host.Model;

public class HostOptions
{
    public int FirstSeed { get; set; }
    public bool AdjustWindow {  get; set; }
    public Rect WindowRect { get; set; }
    public required TimeSpan HostFallAsleepMessageDelay { get; set; }
    public required TimeSpan SkipAnimationDelay { get; set; }    
    public required int PresetPlayerCount { get; set; }
    public required int PresetPlayerSelectedCount { get; set; }
    public required Color CityColor { get; set; }
    public required GroupColor[] GroupColors { get; set; }
    public required Message[] Messages { get; set; }
}