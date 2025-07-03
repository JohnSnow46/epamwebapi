namespace Gamestore.Services.Services.Auth;
public static class Roles
{
    public const string Administrator = "Administrator";
    public const string Manager = "Manager";
    public const string Moderator = "Moderator";
    public const string User = "User";
    public const string Guest = "Guest";

    public static readonly string[] AllRoles =
    [
        Administrator,
        Manager,
        Moderator,
        User,
        Guest
    ];

    public static bool HasPermission(string userRole, string requiredRole)
    {
        return GetRoleLevel(userRole) <= GetRoleLevel(requiredRole);
    }

    private static int GetRoleLevel(string role)
    {
        return role switch
        {
            Administrator => 0,
            Manager => 1,
            Moderator => 2,
            User => 3,
            Guest => 4,
            _ => 4
        };
    }
}
