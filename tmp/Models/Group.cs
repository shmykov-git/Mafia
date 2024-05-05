namespace tmp.Models;

public class Group
{
    public required string Name { get; set; }
    public string[]? Roles { get; set; }
    public Act? Act { get; set; }
    public bool Skippable { get; set; }

    public bool IsCivilian { get; set; }
    public bool IsMafia { get; set; }
    public bool IsCity { get; set; }
    public bool IsManiac { get; set; }


    public GameEnd End => IsMafia ? GameEnd.Mafia : IsManiac ? GameEnd.Maniac : GameEnd.Civilian;
    public bool IsParticipant => IsMafia || IsManiac || IsCivilian;
}
