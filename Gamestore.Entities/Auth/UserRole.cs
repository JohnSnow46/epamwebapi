namespace Gamestore.Entities.Auth;

/// <summary>
/// Represents the many-to-many relationship between User and Role entities in the authorization system.
/// This junction table entity enables the assignment of roles to users,
/// supporting role-based access control (RBAC) with auditing capabilities for role assignments.
/// </summary>
public class UserRole
{
    /// <summary>
    /// Gets or sets the unique identifier for the user-role relationship.
    /// This serves as the primary key for the junction table entity.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user in this relationship.
    /// This is a foreign key reference to the User entity.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the User entity that this relationship references.
    /// This navigation property provides access to the complete user information
    /// including user profile, authentication details, and account status.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the unique identifier of the role in this relationship.
    /// This is a foreign key reference to the Role entity.
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// Gets or sets the Role entity that this relationship references.
    /// This navigation property provides access to the complete role information
    /// including role name, description, permissions, and hierarchy level.
    /// </summary>
    public Role Role { get; set; } = null!;

    /// <summary>
    /// Gets or sets the timestamp when the role was assigned to the user.
    /// This is automatically set to the current UTC time when the relationship is created,
    /// providing audit information for role management and security tracking.
    /// </summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the unique identifier of the administrator who assigned this role to the user.
    /// This is optional and can be null for system-assigned roles or automated processes.
    /// Provides audit trail information for accountability and role management oversight.
    /// </summary>
    public Guid? AssignedBy { get; set; } // Who assigned this role
}