using Gamestore.Data.Data;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repositories;

/// <summary>
/// Repository implementation for managing Role entities in the authorization system.
/// Provides concrete implementations for role-based access control, permission management,
/// and role hierarchy operations with case-insensitive role name handling and eager loading support.
/// Inherits from the generic Repository pattern and implements IRoleRepository interface.
/// </summary>
public class RoleRepository(GameCatalogDbContext context) : Repository<Role>(context), IRoleRepository
{
    private readonly GameCatalogDbContext _context = context;

    /// <summary>
    /// Retrieves a role by its unique name identifier using case-insensitive comparison.
    /// Role names serve as string-based identifiers for authorization checks
    /// and role assignment operations with flexible case handling.
    /// </summary>
    /// <param name="name">The unique name of the role to retrieve. Case-insensitive lookup.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the Role entity
    /// if found, or null if no role with the specified name exists.
    /// </returns>
    public async Task<Role?> GetByNameAsync(string name)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Name.ToLower() == name.ToLower());
    }

    /// <summary>
    /// Retrieves a role by its name including all associated permissions using case-insensitive comparison.
    /// This method eagerly loads the role's permissions through the RolePermission relationship
    /// for scenarios where immediate access to role capabilities is required without additional queries.
    /// </summary>
    /// <param name="name">The unique name of the role to retrieve with permissions. Case-insensitive lookup.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the Role entity
    /// with loaded permissions if found, or null if no role with the specified name exists.
    /// </returns>
    public async Task<Role?> GetByNameWithPermissionsAsync(string name)
    {
        return await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Name.ToLower() == name.ToLower());
    }

    /// <summary>
    /// Retrieves all roles in the system including their associated permissions.
    /// This method provides a complete view of the role hierarchy and permission
    /// structure for administrative interfaces and role management operations.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection
    /// of all Role entities with their permissions loaded through the RolePermission relationship.
    /// Returns an empty collection if no roles exist.
    /// </returns>
    public async Task<IEnumerable<Role>> GetAllWithPermissionsAsync()
    {
        return await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .ToListAsync();
    }

    /// <summary>
    /// Checks whether a role with the specified name exists in the system using case-insensitive comparison.
    /// This method provides an efficient way to validate role names without retrieving
    /// the entire role entity, useful for role validation and duplicate checking.
    /// </summary>
    /// <param name="name">The role name to check for existence. Case-insensitive lookup.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is true if a role
    /// with the specified name exists, false otherwise.
    /// </returns>
    public async Task<bool> RoleExistsAsync(string name)
    {
        return await _context.Roles
            .AnyAsync(r => r.Name.ToLower() == name.ToLower());
    }

    /// <summary>
    /// Retrieves all permissions associated with a specific role.
    /// This method queries through the RolePermission relationship to return
    /// the complete set of permissions granted by the specified role for authorization operations.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role whose permissions to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of Permission entities
    /// associated with the specified role. Returns an empty collection if the role has no permissions.
    /// </returns>
    public async Task<IEnumerable<Permission>> GetRolePermissionsAsync(Guid roleId)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permission)
            .ToListAsync();
    }
}