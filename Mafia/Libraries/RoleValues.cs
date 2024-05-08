namespace Mafia.Libraries;

public static class RoleValues
{
    public static (string name, int count)[] GetRolesPreset(string[] singleRoles, string mafiaRole, string civilianRole, int n, double k = 3.5)
    {
        var nn = n - singleRoles.Length;
        var nMafia = (int)(nn / k);
        var nCivilian = nn - nMafia;

        return singleRoles.Select(r => (r, 1)).Concat([(mafiaRole, nMafia), (civilianRole, nCivilian)]).ToArray();
    }
}
