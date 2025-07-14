using System.ComponentModel.DataAnnotations;

namespace Gamestore.Entities.Auth;

/// <summary>
/// Represents a permission entity in the authorization system that defines specific access rights and capabilities.
/// Permissions are used to control fine-grained access to system features and operations,
/// and can be assigned to roles for role-based access control (RBAC) implementation.
/// </summary>
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public class Permission
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    /// <summary>
    /// Gets or sets the unique identifier for the permission.
    /// This serves as the primary key for the permission entity.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the unique name of the permission.
    /// This is used as a string-based identifier for authorization checks throughout the application.
    /// Examples include "CanEditGames", "CanViewReports", "CanManageUsers".
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a human-readable description of what the permission allows.
    /// This provides detailed information about the permission's purpose and scope
    /// for administrative interfaces and documentation.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category that groups related permissions together.
    /// Categories help organize permissions for better management and understanding.
    /// Examples include "Games", "Users", "Comments", "Orders", "Reports".
    /// </summary>
    public string Category { get; set; } = string.Empty; // Games, Users, Comments, etc.

    // Navigation properties
    /// <summary>
    /// Gets or sets the collection of RolePermission relationships that associate this permission with roles.
    /// This navigation property enables the many-to-many relationship between permissions and roles,
    /// allowing permissions to be assigned to multiple roles for role-based access control.
    /// </summary>
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}