using Gamestore.Entities.Auth;

namespace Gamestore.Data.Interfaces;

/// <summary>
/// Repository interface for managing Permission entities in the authorization system.
/// Provides functionality for permission-based access control, role management, and security validation.
/// Supports permission categorization and role-based permission assignment for fine-grained access control.
/// Extends the generic repository pattern with authorization-specific operations.
/// </summary>
public interface IPermissionRepository : IRepository<Permission>
{
    /// <summary>
    /// Retrieves a specific permission by its unique name identifier.
    /// Permission names are typically used as string-based identifiers for access control checks
    /// throughout the application (e.g., "CanEditGames", "CanViewReports").
    /// </summary>
    /// <param name="name">The unique name of the permission to retrieve. Case-sensitive lookup.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the Permission entity
    /// if found, or null if no permission with the specified name exists.
    /// </returns>
    Task<Permission?> GetByNameAsync(string name);

    /// <summary>
    /// Retrieves all permissions that belong to a specific category.
    /// Categories are used to group related permissions for better organization and management
    /// (e.g., "GameManagement", "UserAdministration", "OrderProcessing").
    /// </summary>
    /// <param name="category">The category name to filter permissions by. Case-sensitive lookup.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of Permission entities
    /// belonging to the specified category. Returns an empty collection if no permissions exist in the category.
    /// </returns>
    Task<IEnumerable<Permission>> GetByCategoryAsync(string category);

    /// <summary>
    /// Retrieves all permissions that are assigned to a specific role.
    /// This method queries through the RolePermission relationship table to find all permissions
    /// associated with the given role, enabling role-based access control validation.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role to retrieve permissions for.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of Permission entities
    /// assigned to the specified role. Returns an empty collection if the role has no permissions assigned.
    /// </returns>
    Task<IEnumerable<Permission>> GetPermissionsByRoleAsync(Guid roleId);

    /// <summary>
    /// Checks whether a permission with the specified name exists in the system.
    /// This method provides an efficient way to validate permission names without retrieving
    /// the entire permission entity, useful for permission validation and duplicate checking.
    /// </summary>
    /// <param name="name">The permission name to check for existence. Case-sensitive lookup.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is true if a permission
    /// with the specified name exists, false otherwise.
    /// </returns>
    Task<bool> PermissionExistsAsync(string name);
}