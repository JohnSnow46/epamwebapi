namespace Gamestore.Entities.Auth;

/// <summary>
/// Represents the many-to-many relationship between Role and Permission entities in the authorization system.
/// This junction table entity enables the assignment of specific permissions to roles,
/// supporting role-based access control (RBAC) with auditing capabilities for permission grants.
/// </summary>
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public class RolePermission
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    /// <summary>
    /// Gets or sets the unique identifier for the role-permission relationship.
    /// This serves as the primary key for the junction table entity.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the role in this relationship.
    /// This is a foreign key reference to the Role entity.
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// Gets or sets the Role entity that this relationship references.
    /// This navigation property provides access to the complete role information
    /// including role name, description, and hierarchy level.
    /// </summary>
    public Role Role { get; set; } = null!;

    /// <summary>
    /// Gets or sets the unique identifier of the permission in this relationship.
    /// This is a foreign key reference to the Permission entity.
    /// </summary>
    public Guid PermissionId { get; set; }

    /// <summary>
    /// Gets or sets the Permission entity that this relationship references.
    /// This navigation property provides access to the complete permission information
    /// including permission name, description, and category.
    /// </summary>
    public Permission Permission { get; set; } = null!;

    /// <summary>
    /// Gets or sets the timestamp when the permission was granted to the role.
    /// This is automatically set to the current UTC time when the relationship is created,
    /// providing audit information for permission management and security tracking.
    /// </summary>
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
}