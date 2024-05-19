using Mafia.Extensions;
using Mafia.Model;

namespace Mafia.Libraries;

public static class RoleValues
{
    public static (string name, int count)[] GetRolePreset(RolePreset preset, int n)
    {
        if (preset.Ratios.Length != preset.MultipleRoles.Length)
            throw new InvalidOperationException("Incorrect preset");

        List<(string name, int count)> fullHouse = preset.SingleRoles.Select(r => (r, 1)).ToList();

        var nn = n - preset.SingleRoles.Length;
        var ratios = preset.Ratios.Select(r => r / preset.Ratios.Sum()).ToArray();

        for (var i = 0; i < ratios.Length - 1; i++)
        {
            var k = (int)Math.Round(nn * ratios[i]);
            fullHouse.Add((preset.MultipleRoles[i], k));
        }

        fullHouse.Add((preset.MultipleRoles[^1], n - fullHouse.Sum(v => v.count)));

        return fullHouse.ToArray();
    }
}
