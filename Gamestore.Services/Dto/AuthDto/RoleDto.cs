namespace Gamestore.Services.Dto.AuthDto;

/// <summary>
/// Represents a role data transfer object for API responses.
/// Contains role information used in client-server communication.
/// </summary>
public class RoleDto
{
    /// <summary>
    /// Gets or sets the name of the role.
    /// This is the display name and identifier for the role.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier of the role.
    /// This serves as the primary key for the role entity.
    /// </summary>
    public string Id { get; set; } = string.Empty;
}
