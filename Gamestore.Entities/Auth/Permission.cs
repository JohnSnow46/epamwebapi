using System.ComponentModel.DataAnnotations;

namespace Gamestore.Entities.Auth;
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public class Permission
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty; // Games, Users, Comments, etc.

    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}