using System.ComponentModel.DataAnnotations;

namespace Gamestore.Entities.Auth;

/// <summary>
/// Represents a role entity that defines a set of permissions.
/// </summary>
public class Role
{
    /// <summary>
    /// Gets or sets the unique identifier for the role.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the unique name of the role.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a description of the role's purpose.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hierarchical level of the role.
    /// </summary>
    public int Level { get; set; } // 0 = highest (Admin), 4 = lowest (Guest)

    /// <summary>
    /// Gets or sets a value indicating whether this is a system-defined role.
    /// </summary>
    public bool IsSystemRole { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the role was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    /// <summary>
    /// Gets or sets the collection of user-role relationships.
    /// </summary>
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    /// <summary>
    /// Gets or sets the collection of role-permission relationships.
    /// </summary>
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}