using Gamestore.Data.Data;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repositories;

/// <summary>
/// Repository implementation for managing Permission entities in the authorization system.
/// Provides concrete implementations for permission-based access control, role management,
/// and security validation with case-insensitive operations for permission names and categories.
/// Inherits from the generic Repository pattern and implements IPermissionRepository interface.
/// </summary>
public class PermissionRepository(GameCatalogDbContext context) : Repository<Permission>(context), IPermissionRepository
{
    private readonly GameCatalogDbContext _context = context;

    /// <summary>
    /// Retrieves a specific permission by its unique name identifier using case-insensitive comparison.
    /// Permission names are used as string-based identifiers for access control checks
    /// throughout the application with flexible case handling.
    /// </summary>
    /// <param name="name">The unique name of the permission to retrieve. Case-insensitive lookup.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the Permission entity
    /// if found, or null if no permission with the specified name exists.
    /// </returns>
    public async Task<Permission?> GetByNameAsync(string name)
    {
        return await _context.Permissions
            .FirstOrDefaultAsync(p => p.Name.ToLower() == name.ToLower());
    }

    /// <summary>
    /// Retrieves all permissions that belong to a specific category using case-insensitive comparison.
    /// Categories are used to group related permissions for better organization and management
    /// with flexible case handling for category names.
    /// </summary>
    /// <param name="category">The category name to filter permissions by. Case-insensitive lookup.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of Permission entities
    /// belonging to the specified category. Returns an empty collection if no permissions exist in the category.
    /// </returns>
    public async Task<IEnumerable<Permission>> GetByCategoryAsync(string category)
    {
        return await _context.Permissions
            .Where(p => p.Category.ToLower() == category.ToLower())
            .ToListAsync();
    }

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
    public async Task<IEnumerable<Permission>> GetPermissionsByRoleAsync(Guid roleId)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permission)
            .ToListAsync();
    }

    /// <summary>
    /// Checks whether a permission with the specified name exists in the system using case-insensitive comparison.
    /// This method provides an efficient way to validate permission names without retrieving
    /// the entire permission entity, useful for permission validation and duplicate checking.
    /// </summary>
    /// <param name="name">The permission name to check for existence. Case-insensitive lookup.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is true if a permission
    /// with the specified name exists, false otherwise.
    /// </returns>
    public async Task<bool> PermissionExistsAsync(string name)
    {
        return await _context.Permissions
            .AnyAsync(p => p.Name.ToLower() == name.ToLower());
    }
}