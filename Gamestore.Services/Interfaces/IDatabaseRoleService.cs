using Gamestore.Entities.Auth;

namespace Gamestore.Services.Interfaces;

public interface IDatabaseRoleService
{
    // ========== CORE USER OPERATIONS (Authentication) ==========
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(Guid id);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User> CreateUserAsync(string email, string firstName, string lastName, string passwordHash, string? roleName = null);
    Task<User> UpdateUserAsync(Guid userId, string? firstName = null, string? lastName = null, string? email = null);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<bool> UserExistsAsync(string email);
    Task<bool> ValidateUserPasswordAsync(string email, string password);

    // ========== CORE ROLE OPERATIONS (Authorization) ==========
    Task<string> GetUserRoleAsync(string email);
    Task<IEnumerable<string>> GetUserRolesAsync(string email);
    Task<bool> AssignUserToRoleAsync(string email, string roleName, Guid? assignedBy = null);
    Task<bool> RemoveUserFromRoleAsync(string email, string roleName);
    Task<bool> UserHasRoleAsync(string email, string roleName);
    Task<bool> UserHasPermissionAsync(string email, string permissionName);

    // ========== CORE ROLE DEFINITIONS (System Operations) ==========
    Task<IEnumerable<Role>> GetAllRolesAsync();
    Task<Role?> GetRoleByNameAsync(string name);
    Task<Role> CreateRoleAsync(string name, string description, int level, bool isSystemRole = false);
    Task<bool> DeleteRoleAsync(string name);
    Task<bool> RoleExistsAsync(string name);

    // ========== CORE PERMISSION OPERATIONS (System Operations) ==========
    Task<IEnumerable<Permission>> GetAllPermissionsAsync();
    Task<IEnumerable<Permission>> GetRolePermissionsAsync(string roleName);
    Task<bool> AssignPermissionToRoleAsync(string roleName, string permissionName);
    Task<bool> RemovePermissionFromRoleAsync(string roleName, string permissionName);

    // ========== SYSTEM INITIALIZATION ==========
    Task SeedDefaultRolesAndPermissionsAsync();
    Task MigrateHardcodedUsersAsync();
}