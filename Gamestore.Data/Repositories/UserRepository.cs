using Gamestore.Data.Data;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repositories;

/// <summary>
/// Repository implementation for managing User entities in the authentication and authorization system.
/// Provides concrete implementations for user account management, authentication operations, role assignments,
/// and user activity tracking with case-insensitive email handling and eager loading support.
/// Inherits from the generic Repository pattern and implements IUserRepository interface.
/// </summary>
public class UserRepository(GameCatalogDbContext context) : Repository<User>(context), IUserRepository
{
    private readonly GameCatalogDbContext _context = context;

    /// <summary>
    /// Retrieves a user by their unique email address using case-insensitive comparison.
    /// Email addresses serve as the primary login identifier for user authentication
    /// with flexible case handling for improved user experience.
    /// </summary>
    /// <param name="email">The email address of the user to retrieve. Case-insensitive lookup.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the User entity
    /// if found, or null if no user with the specified email exists.
    /// </returns>
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    /// <summary>
    /// Retrieves a user by their email address including all assigned roles using case-insensitive comparison.
    /// This method eagerly loads the user's roles through the UserRole relationship for scenarios
    /// where immediate access to user permissions and role information is required for authorization checks.
    /// </summary>
    /// <param name="email">The email address of the user to retrieve with roles. Case-insensitive lookup.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the User entity
    /// with loaded roles if found, or null if no user with the specified email exists.
    /// </returns>
    public async Task<User?> GetByEmailWithRolesAsync(string email)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    /// <summary>
    /// Retrieves a user by their unique identifier including all assigned roles.
    /// This method provides complete user information with role data for authorization
    /// and user management operations using eager loading for efficiency.
    /// </summary>
    /// <param name="id">The unique identifier of the user to retrieve with roles.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the User entity
    /// with loaded roles if found, or null if no user with the specified ID exists.
    /// </returns>
    public async Task<User?> GetByIdWithRolesAsync(Guid id)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    /// <summary>
    /// Retrieves all users in the system including their assigned roles.
    /// This method provides a complete view of all user accounts and their role assignments
    /// for administrative interfaces and user management operations with comprehensive data loading.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection
    /// of all User entities with their roles loaded through the UserRole relationship.
    /// Returns an empty collection if no users exist.
    /// </returns>
    public async Task<IEnumerable<User>> GetAllWithRolesAsync()
    {
        return await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .ToListAsync();
    }

    /// <summary>
    /// Checks whether a user with the specified email address exists in the system using case-insensitive comparison.
    /// This method provides an efficient way to validate email uniqueness without retrieving
    /// the entire user entity, useful for registration validation and duplicate checking.
    /// </summary>
    /// <param name="email">The email address to check for existence. Case-insensitive lookup.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is true if a user
    /// with the specified email exists, false otherwise.
    /// </returns>
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }

    /// <summary>
    /// Retrieves all users that have been assigned a specific role.
    /// This method queries through the UserRole relationship to find users with the specified role assignment,
    /// useful for role-based user management and administrative operations.
    /// </summary>
    /// <param name="roleName">The name of the role to find users for.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of User entities
    /// with roles loaded, filtered to those assigned the specified role. Returns an empty collection if no users have the specified role.
    /// </returns>
    public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == roleName))
            .ToListAsync();
    }

    /// <summary>
    /// Updates the last login timestamp for a specific user to track user activity.
    /// This method efficiently updates only the LastLoginAt property without loading
    /// the entire user entity, providing optimized tracking for security monitoring and analytics.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose last login time to update.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
    public async Task UpdateLastLoginAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}