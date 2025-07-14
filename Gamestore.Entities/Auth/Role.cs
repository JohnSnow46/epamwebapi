using System.ComponentModel.DataAnnotations;

namespace Gamestore.Entities.Auth;

/// <summary>
/// Represents a role entity in the authorization system that defines a set of permissions and access levels.
/// Roles are used to group permissions and assign them to users for role-based access control (RBAC),
/// enabling hierarchical permission management and simplified user administration.
/// </summary>
public class Role
{
    /// <summary>
    /// Gets or sets the unique identifier for the role.
    /// This serves as the primary key for the role entity.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the unique name of the role.
    /// This is used as a string-based identifier for role assignment and authorization checks.
    /// Examples include "Administrator", "Manager", "User", "Guest".
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a human-readable description of the role's purpose and capabilities.
    /// This provides detailed information about the role's responsibilities and scope
    /// for administrative interfaces and documentation.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hierarchical level of the role for authorization precedence.
    /// Lower numbers indicate higher authority levels (0 = highest authority like Admin).
    /// Higher numbers indicate lower authority levels (4 = lowest authority like Guest).
    /// This enables role-based hierarchy enforcement and permission inheritance.
    /// </summary>
    public int Level { get; set; } // 0 = highest (Admin), 4 = lowest (Guest)

    /// <summary>
    /// Gets or sets a value indicating whether this role is a system-defined role.
    /// System roles are protected from deletion or modification to maintain system integrity.
    /// Examples of system roles include built-in Administrator, User, and Guest roles.
    /// </summary>
    public bool IsSystemRole { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the role was created.
    /// This is automatically set to the current UTC time when the role is instantiated,
    /// providing audit information for role management.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    /// <summary>
    /// Gets or sets the collection of UserRole relationships that associate users with this role.
    /// This navigation property enables the many-to-many relationship between users and roles,
    /// allowing multiple users to be assigned to this role.
    /// </summary>
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    /// <summary>
    /// Gets or sets the collection of RolePermission relationships that associate permissions with this role.
    /// This navigation property enables the many-to-many relationship between roles and permissions,
    /// allowing multiple permissions to be assigned to this role for access control.
    /// </summary>
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}