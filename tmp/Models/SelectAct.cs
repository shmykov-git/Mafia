namespace tmp.Models;

public class SelectAct
{
    public required string Role { get; set; }
    public required Act Act { get; set; }
    public bool SelfOnes { get; set; }
    public bool Skippable { get; set; }
}
