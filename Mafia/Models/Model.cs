using System.Reflection;

namespace Mafia.Models;

public class Model
{
    public required Group[] Groups { get; set; }
    public required SelectAct[] SelectActs { get; set; }

    public required Event[] FirstDayEvents { get; set; }
    public required Event[] DayEvents { get; set; }
    public required ActEvent[] OnDeathEvents { get; set; }

    public string[] Roles => Groups.Where(g => g.Roles != null).SelectMany(g => g.Roles!).Distinct().ToArray();
    public Group? GetGroupByRole(string role) => Groups.Where(g => g.Roles != null && g.IsAct).SingleOrDefault(g => g.Roles!.Contains(role));
}
