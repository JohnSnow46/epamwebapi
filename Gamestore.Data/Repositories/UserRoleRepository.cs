using Gamestore.Data.Data;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repositories;

/// <summary>
/// Repository implementation for managing UserRole entities in the authorization system.
/// Provides concrete implementations for the many-to-many relationships between users and roles,
/// including role assignment, role removal, bulk operations, and auditing support.
/// Inherits from the generic Repository pattern and implements IUserRoleRepository interface.
/// </summary>
public class UserRoleRepository(GameCatalogDbContext context) : Repository<UserRole>(context), IUserRoleRepository
{
    private readonly GameCatalogDbContext _context = context;

    /// <summary>
    /// Retrieves all role assignments for a specific user with role details eagerly loaded.
    /// This method returns all roles that have been assigned to the specified user,
    /// useful for authorization checks and user profile management with complete role information.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose role assignments to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of UserRole entities
    /// with role details loaded, representing all role assignments for the specified user.
    /// Returns an empty collection if the user has no roles.
    /// </returns>
    public async Task<IEnumerable<UserRole>> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves all user assignments for a specific role with user details eagerly loaded.
    /// This method returns all users that have been assigned the specified role,
    /// useful for role management and user administration operations with complete user information.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role whose user assignments to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of UserRole entities
    /// with user details loaded, representing all user assignments for the specified role.
    /// Returns an empty collection if no users have this role.
    /// </returns>
    public async Task<IEnumerable<UserRole>> GetByRoleIdAsync(Guid roleId)
    {
        return await _context.UserRoles
            .Include(ur => ur.User)
            .Where(ur => ur.RoleId == roleId)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves a specific user-role relationship if it exists.
    /// This method checks whether a specific user has been assigned a specific role,
    /// useful for role validation and permission checking operations without loading additional data.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the UserRole entity
    /// if the relationship exists, or null if the user is not assigned to the specified role.
    /// </returns>
    public async Task<UserRole?> GetUserRoleAsync(Guid userId, Guid roleId)
    {
        return await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
    }

    /// <summary>
    /// Assigns a role to a user, creating a new user-role relationship with auditing information.
    /// This method establishes the many-to-many relationship between a user and a role,
    /// with duplicate assignment prevention and optional auditing about who performed the assignment.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to assign the role to.</param>
    /// <param name="roleId">The unique identifier of the role to assign to the user.</param>
    /// <param name="assignedBy">Optional unique identifier of the administrator who performed the role assignment for auditing purposes.</param>
    /// <returns>A task representing the asynchronous assignment operation.</returns>
    public async Task AddUserToRoleAsync(Guid userId, Guid roleId, Guid? assignedBy = null)
    {
        var existingUserRole = await GetUserRoleAsync(userId, roleId);
        if (existingUserRole == null)
        {
            var userRole = new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RoleId = roleId,
                AssignedBy = assignedBy,
                AssignedAt = DateTime.UtcNow
            };

            await _context.UserRoles.AddAsync(userRole);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Removes a specific role assignment from a user.
    /// This method removes the relationship between a user and a role,
    /// effectively revoking the role's permissions from the user with immediate persistence.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to remove the role from.</param>
    /// <param name="roleId">The unique identifier of the role to remove from the user.</param>
    /// <returns>A task representing the asynchronous removal operation.</returns>
    public async Task RemoveUserFromRoleAsync(Guid userId, Guid roleId)
    {
        var userRole = await GetUserRoleAsync(userId, roleId);
        if (userRole != null)
        {
            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Removes all role assignments from a specific user in a single bulk operation.
    /// This method performs a bulk removal of all roles assigned to a user,
    /// typically used when deactivating or deleting user accounts with immediate persistence.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose role assignments to remove.</param>
    /// <returns>A task representing the asynchronous bulk removal operation.</returns>
    public async Task RemoveAllUserRolesAsync(Guid userId)
    {
        var userRoles = await GetByUserIdAsync(userId);
        _context.UserRoles.RemoveRange(userRoles);
        await _context.SaveChangesAsync();
    }
}