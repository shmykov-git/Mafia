using System.Reflection;

namespace Mafia.Models;

public class Model
{
    public required Group[] Groups { get; set; }
    public required SelectAct[] SelectActs { get; set; }

    public required Event[] FirstDayEvents { get; set; }
    public required Event[] DayEvents { get; set; }
    public required ActEvent[] OnDeathEvents { get; set; }

    public string[] Roles => Groups.Where(g => g.HasSingleRole && g.Roles != null).OrderBy(g => g.Name).SelectMany(g => g.Roles!).ToArray();
    public Group? GetGroupByRole(string role) => Groups.Where(g => g.Roles != null && g.IsAct).SingleOrDefault(g => g.Roles!.Contains(role));
}
