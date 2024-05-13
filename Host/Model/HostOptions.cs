using Mafia.Model;

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
    public required RolePreset RolePreset { get; set; }
    public required Color CityColor { get; set; }
    public required Color WakeupColor { get; set; }
    public required Color FallAsleepColor { get; set; }    
    public required Color UnwantedColor { get; set; }
    public required Color NoOperationColor { get; set; }
    public required Color KilledColor { get; set; }
    
    public required GroupColor[] GroupColors { get; set; }
    public required OperationColor[] OperationColors { get; set; }
    
    public required Message[] Messages { get; set; }
}
