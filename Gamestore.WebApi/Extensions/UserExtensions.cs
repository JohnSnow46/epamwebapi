using Gamestore.Services.Interfaces;
using Gamestore.Services.Services.Auth;
using System.Security.Claims;

namespace Gamestore.WebApi.Extensions;

public static class UserExtensions
{
    public static string GetUserRole(this ClaimsPrincipal user)
    {
        return user.FindFirst("role")?.Value ??
               user.FindFirst(ClaimTypes.Role)?.Value ??
               Roles.Guest;
    }

    public static string GetUserEmail(this ClaimsPrincipal user)
    {
        return user.FindFirst("email")?.Value ??
               user.FindFirst(ClaimTypes.Email)?.Value ??
               user.FindFirst("unique_name")?.Value ??
               string.Empty;
    }

    public static string GetUserName(this ClaimsPrincipal user)
    {
        return user.FindFirst("unique_name")?.Value ??
               user.FindFirst(ClaimTypes.Name)?.Value ??
               user.FindFirst("name")?.Value ??
               "Anonymous";
    }

    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var userIdString = user.FindFirst("user_id")?.Value ??
                          user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                          user.FindFirst("sub")?.Value;

        return Guid.TryParse(userIdString, out var userId) ? userId : null;
    }

    public static bool IsAuthenticated(this ClaimsPrincipal user)
    {
        return user.Identity?.IsAuthenticated == true;
    }

    // Role checking methods
    public static bool IsAdmin(this ClaimsPrincipal user)
        => user.GetUserRole() == Roles.Administrator;

    public static bool IsManager(this ClaimsPrincipal user)
        => user.GetUserRole() == Roles.Manager;

    public static bool IsModerator(this ClaimsPrincipal user)
        => user.GetUserRole() == Roles.Moderator;

    public static bool IsUser(this ClaimsPrincipal user)
        => user.GetUserRole() == Roles.User;

    public static bool IsGuest(this ClaimsPrincipal user)
        => user.GetUserRole() == Roles.Guest;

    // Permission checking methods based on Epic 9 requirements
    public static bool CanManageUsers(this ClaimsPrincipal user)
        => user.IsAdmin();

    public static bool CanManageRoles(this ClaimsPrincipal user)
        => user.IsAdmin();

    public static bool CanManageGames(this ClaimsPrincipal user)
        => user.IsAdmin() || user.IsManager();

    public static bool CanManageBusinessEntities(this ClaimsPrincipal user)
        => user.IsAdmin() || user.IsManager();

    public static bool CanViewDeletedGames(this ClaimsPrincipal user)
        => user.IsAdmin();

    public static bool CanEditDeletedGames(this ClaimsPrincipal user)
        => user.IsAdmin();

    public static bool CanManageOrders(this ClaimsPrincipal user)
        => user.IsAdmin() || user.IsManager();

    public static bool CanViewOrderHistory(this ClaimsPrincipal user)
        => user.IsAdmin() || user.IsManager();

    public static bool CanShipOrders(this ClaimsPrincipal user)
        => user.IsAdmin() || user.IsManager();

    public static bool CanModerateComments(this ClaimsPrincipal user)
        => user.IsAdmin() || user.IsManager() || user.IsModerator();

    public static bool CanBanUsers(this ClaimsPrincipal user)
        => user.IsAdmin() || user.IsManager() || user.IsModerator();

    public static bool CanComment(this ClaimsPrincipal user)
        => user.IsAuthenticated() && user.GetUserRole() != Roles.Guest;

    public static bool CanBuyGames(this ClaimsPrincipal user)
        => user.IsAuthenticated() && user.GetUserRole() != Roles.Guest;

    public static bool HasReadOnlyAccess(this ClaimsPrincipal user)
        => user.GetUserRole() == Roles.Guest || !user.IsAuthenticated();

    // Helper method to check role hierarchy
    public static bool HasRoleOrHigher(this ClaimsPrincipal user, string requiredRole)
    {
        var userRole = user.GetUserRole();
        return Roles.HasPermission(userRole, requiredRole);
    }

    // New method for database-based permission checking
    public static async Task<bool> HasPermissionAsync(this ClaimsPrincipal user, IDatabaseRoleService roleService, string permission)
    {
        var email = user.GetUserEmail();
        return !string.IsNullOrEmpty(email) && await roleService.UserHasPermissionAsync(email, permission);
    }
}