namespace Mafia.Model;

public class Select
{
    public required string Operation { get; set; }
    public required Player Who { get; set; }
    public required Player[] Whom { get; set; }

    public bool IsCity => Who == null;
}
