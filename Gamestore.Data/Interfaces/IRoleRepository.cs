using Gamestore.Entities.Auth;

namespace Gamestore.Data.Interfaces;

/// <summary>
/// Repository interface for managing Role entities in the authorization system.
/// Provides functionality for role-based access control, permission management,
/// and role hierarchy operations. Supports eager loading of role permissions
/// for efficient authorization checking.
/// Extends the generic repository pattern with role-specific operations.
/// </summary>
public interface IRoleRepository : IRepository<Role>
{
    /// <summary>
    /// Retrieves a role by its unique name identifier.
    /// Role names serve as string-based identifiers for authorization checks
    /// and role assignment operations throughout the application.
    /// </summary>
    /// <param name="name">The unique name of the role to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the Role entity
    /// if found, or null if no role with the specified name exists.
    /// </returns>
    Task<Role?> GetByNameAsync(string name);

    /// <summary>
    /// Retrieves a role by its name including all associated permissions.
    /// This method eagerly loads the role's permissions for scenarios where
    /// immediate access to role capabilities is required without additional queries.
    /// </summary>
    /// <param name="name">The unique name of the role to retrieve with permissions.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the Role entity
    /// with loaded permissions if found, or null if no role with the specified name exists.
    /// </returns>
    Task<Role?> GetByNameWithPermissionsAsync(string name);

    /// <summary>
    /// Retrieves all roles in the system including their associated permissions.
    /// This method provides a complete view of the role hierarchy and permission
    /// structure for administrative interfaces and role management operations.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection
    /// of all Role entities with their permissions loaded. Returns an empty collection if no roles exist.
    /// </returns>
    Task<IEnumerable<Role>> GetAllWithPermissionsAsync();

    /// <summary>
    /// Checks whether a role with the specified name exists in the system.
    /// This method provides an efficient way to validate role names without retrieving
    /// the entire role entity, useful for role validation and duplicate checking.
    /// </summary>
    /// <param name="name">The role name to check for existence.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is true if a role
    /// with the specified name exists, false otherwise.
    /// </returns>
    Task<bool> RoleExistsAsync(string name);

    /// <summary>
    /// Retrieves all permissions associated with a specific role.
    /// This method queries through the RolePermission relationship to return
    /// the complete set of permissions granted by the specified role.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role whose permissions to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of Permission entities
    /// associated with the specified role. Returns an empty collection if the role has no permissions.
    /// </returns>
    Task<IEnumerable<Permission>> GetRolePermissionsAsync(Guid roleId);
}