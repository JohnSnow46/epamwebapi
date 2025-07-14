using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.AuthDto;

/// <summary>
/// Represents the data transfer object for creating a new role.
/// Contains the basic information required to create a role in the authorization system.
/// </summary>
public class RoleCreateDto
{
    /// <summary>
    /// Gets or sets the name of the role to be created.
    /// This serves as the unique identifier and display name for the role.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;
}
