namespace Host.Libraries;

public static class HostValues
{
    public const string SettingsFileName = "appsettings.json";
    public const string MapFolder = "Resources/Maps";

    public static string UsersView => $"//users/UserView";
    public static string RolesView => $"//roles/RoleView";
    public static string GameView => $"//games/GameView";
    public static string RatingView => $"//ratings/RatingView";
    public static string SettingsView => $"//settings/SettingsView";
}
