using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.AuthDto;

/// <summary>
/// Represents a request to update an existing role with new permissions.
/// Contains the updated role information and associated permissions for role management.
/// </summary>
public class UpdateRoleRequest
{
    /// <summary>
    /// Gets or sets the role update data transfer object.
    /// Contains the updated information for the role to be modified.
    /// </summary>
    [Required]
    public RoleUpdateDto Role { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of permission names to assign to the updated role.
    /// These permissions will replace the existing permissions for the role.
    /// </summary>
    public List<string> Permissions { get; set; } = new();
}
