using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.AuthDto;

/// <summary>
/// Represents the data transfer object for updating an existing role.
/// Contains the role identifier and updated information for role modification operations.
/// </summary>
public class RoleUpdateDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the role to be updated.
    /// This identifies which role should be modified in the system.
    /// </summary>
    [Required]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the updated name for the role.
    /// This will replace the existing role name in the system.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;
}
