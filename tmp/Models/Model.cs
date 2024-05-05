using System.Reflection;

namespace tmp.Models;

public class Model
{
    public required Group[] Groups { get; set; }
    public required SelectAct[] SelectActs { get; set; }

    public required Event[] Events { get; set; }
    public required ActEvent[] OnDeathEvents { get; set; }

    public string[] Roles => Groups.Where(g => g.IsParticipant).OrderBy(g => g.Name).SelectMany(g => g.Roles!).ToArray();
    public Group GetGroupByRole(string role) => Groups.Where(g => g.IsParticipant).Single(g => g.Roles!.Contains(role));
}
