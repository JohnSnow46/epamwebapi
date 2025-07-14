using Gamestore.Entities.Auth;

namespace Gamestore.Data.Interfaces;

/// <summary>
/// Repository interface for managing User entities in the authentication and authorization system.
/// Provides functionality for user account management, authentication operations, role assignments,
/// and user activity tracking. Supports eager loading of user roles for authorization scenarios.
/// Extends the generic repository pattern with user-specific operations.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Retrieves a user by their unique email address.
    /// Email addresses serve as the primary login identifier for user authentication.
    /// </summary>
    /// <param name="email">The email address of the user to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the User entity
    /// if found, or null if no user with the specified email exists.
    /// </returns>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Retrieves a user by their email address including all assigned roles.
    /// This method eagerly loads the user's roles for scenarios where immediate access
    /// to user permissions and role information is required for authorization checks.
    /// </summary>
    /// <param name="email">The email address of the user to retrieve with roles.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the User entity
    /// with loaded roles if found, or null if no user with the specified email exists.
    /// </returns>
    Task<User?> GetByEmailWithRolesAsync(string email);

    /// <summary>
    /// Retrieves a user by their unique identifier including all assigned roles.
    /// This method provides complete user information with role data for authorization
    /// and user management operations.
    /// </summary>
    /// <param name="id">The unique identifier of the user to retrieve with roles.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the User entity
    /// with loaded roles if found, or null if no user with the specified ID exists.
    /// </returns>
    Task<User?> GetByIdWithRolesAsync(Guid id);

    /// <summary>
    /// Retrieves all users in the system including their assigned roles.
    /// This method provides a complete view of all user accounts and their role assignments
    /// for administrative interfaces and user management operations.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection
    /// of all User entities with their roles loaded. Returns an empty collection if no users exist.
    /// </returns>
    Task<IEnumerable<User>> GetAllWithRolesAsync();

    /// <summary>
    /// Checks whether a user with the specified email address exists in the system.
    /// This method provides an efficient way to validate email uniqueness without retrieving
    /// the entire user entity, useful for registration validation and duplicate checking.
    /// </summary>
    /// <param name="email">The email address to check for existence.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is true if a user
    /// with the specified email exists, false otherwise.
    /// </returns>
    Task<bool> EmailExistsAsync(string email);

    /// <summary>
    /// Retrieves all users that have been assigned a specific role.
    /// This method queries through the UserRole relationship to find users
    /// with the specified role assignment, useful for role-based user management.
    /// </summary>
    /// <param name="roleName">The name of the role to find users for.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of User entities
    /// assigned to the specified role. Returns an empty collection if no users have the specified role.
    /// </returns>
    Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName);

    /// <summary>
    /// Updates the last login timestamp for a specific user.
    /// This method tracks user activity for security monitoring, analytics,
    /// and account management purposes without loading the entire user entity.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose last login time to update.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
    Task UpdateLastLoginAsync(Guid userId);
}