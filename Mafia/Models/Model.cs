namespace Mafia.Models;

public class Model
{
    public required Group[] Groups { get; set; }
    public required SelectAct[] SelectActs { get; set; }

    public required Event[] FirstDayEvents { get; set; }
    public required Event[] DayEvents { get; set; }
}
