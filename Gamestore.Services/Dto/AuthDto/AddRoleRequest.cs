using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.AuthDto;

/// <summary>
/// Represents a request to add a new role with associated permissions.
/// Used for role management operations in the game store authorization system.
/// </summary>
public class AddRoleRequest
{
    /// <summary>
    /// Gets or sets the role creation data transfer object.
    /// Contains the basic information needed to create a new role.
    /// </summary>
    [Required]
    public RoleCreateDto Role { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of permission names to assign to the new role.
    /// These permissions define what actions the role can perform in the system.
    /// </summary>
    public List<string> Permissions { get; set; }
}