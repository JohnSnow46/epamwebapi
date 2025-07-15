using System.ComponentModel.DataAnnotations;

namespace Gamestore.Entities.Auth;

/// <summary>
/// Represents a permission entity that defines access rights.
/// </summary>
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public class Permission
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    /// <summary>
    /// Gets or sets the unique identifier for the permission.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the unique name of the permission.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a description of what the permission allows.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category that groups related permissions.
    /// </summary>
    public string Category { get; set; } = string.Empty; // Games, Users, Comments, etc.

    /// <summary>
    /// Gets or sets the collection of role-permission relationships.
    /// </summary>
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}