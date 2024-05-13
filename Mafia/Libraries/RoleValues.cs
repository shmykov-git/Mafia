using Mafia.Model;

namespace Mafia.Libraries;

public static class RoleValues
{
    public static (string name, int count)[] GetRolePreset(RolePreset preset, int n)
    {
        var nn = n - preset.SingleRoles.Length;
        var nMafia = (int)(nn / preset.Ratio);
        var nCivilian = nn - nMafia;

        return preset.SingleRoles.Select(r => (r, 1)).Concat([(preset.MafiaRole, nMafia), (preset.CivilianRole, nCivilian)]).ToArray();
    }
}
