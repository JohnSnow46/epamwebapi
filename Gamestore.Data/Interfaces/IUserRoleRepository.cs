using Gamestore.Entities.Auth;

namespace Gamestore.Data.Interfaces;

/// <summary>
/// Repository interface for managing UserRole entities in the authorization system.
/// Handles the many-to-many relationships between users and roles, providing functionality
/// for role assignment, role removal, and role membership queries.
/// Supports auditing of role assignments and bulk role management operations.
/// Extends the generic repository pattern with user-role relationship operations.
/// </summary>
public interface IUserRoleRepository : IRepository<UserRole>
{
    /// <summary>
    /// Retrieves all role assignments for a specific user.
    /// This method returns all roles that have been assigned to the specified user,
    /// useful for authorization checks and user profile management.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose role assignments to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of UserRole entities
    /// representing all role assignments for the specified user. Returns an empty collection if the user has no roles.
    /// </returns>
    Task<IEnumerable<UserRole>> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Retrieves all user assignments for a specific role.
    /// This method returns all users that have been assigned the specified role,
    /// useful for role management and user administration operations.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role whose user assignments to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of UserRole entities
    /// representing all user assignments for the specified role. Returns an empty collection if no users have this role.
    /// </returns>
    Task<IEnumerable<UserRole>> GetByRoleIdAsync(Guid roleId);

    /// <summary>
    /// Retrieves a specific user-role relationship if it exists.
    /// This method checks whether a specific user has been assigned a specific role,
    /// useful for role validation and permission checking operations.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the UserRole entity
    /// if the relationship exists, or null if the user is not assigned to the specified role.
    /// </returns>
    Task<UserRole?> GetUserRoleAsync(Guid userId, Guid roleId);

    /// <summary>
    /// Assigns a role to a user, creating a new user-role relationship.
    /// This method establishes the many-to-many relationship between a user and a role,
    /// with optional auditing information about who performed the assignment.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to assign the role to.</param>
    /// <param name="roleId">The unique identifier of the role to assign to the user.</param>
    /// <param name="assignedBy">Optional unique identifier of the administrator who performed the role assignment for auditing purposes.</param>
    /// <returns>A task representing the asynchronous assignment operation.</returns>
    Task AddUserToRoleAsync(Guid userId, Guid roleId, Guid? assignedBy = null);

    /// <summary>
    /// Removes a specific role assignment from a user.
    /// This method removes the relationship between a user and a role,
    /// effectively revoking the role's permissions from the user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to remove the role from.</param>
    /// <param name="roleId">The unique identifier of the role to remove from the user.</param>
    /// <returns>A task representing the asynchronous removal operation.</returns>
    Task RemoveUserFromRoleAsync(Guid userId, Guid roleId);

    /// <summary>
    /// Removes all role assignments from a specific user.
    /// This method performs a bulk removal of all roles assigned to a user,
    /// typically used when deactivating or deleting user accounts.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose role assignments to remove.</param>
    /// <returns>A task representing the asynchronous bulk removal operation.</returns>
    Task RemoveAllUserRolesAsync(Guid userId);
}