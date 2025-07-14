using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.AuthDto;

/// <summary>
/// Represents the data transfer object for creating a new user.
/// Contains the basic information required to create a user account.
/// </summary>
public class UserCreateDto
{
    /// <summary>
    /// Gets or sets the name of the user to be created.
    /// This serves as the primary identifier and display name for the user.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;
}