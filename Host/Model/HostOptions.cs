namespace Host.Model;

public class HostOptions
{
    public int FirstSeed { get; set; }
    public bool AdjustWindow {  get; set; }
    public Rect WindowRect { get; set; }
    public required GroupColor[] GroupColors { get; set; }
}