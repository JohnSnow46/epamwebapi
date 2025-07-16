using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.AuthDto;

/// <summary>
/// Represents the data transfer object for creating a new user.
/// Contains the basic user information required for user registration.
/// </summary>
public class UserCreateDto
{
    /// <summary>
    /// Gets or sets the user's email address.
    /// This must be a valid email format and serves as the primary identifier.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's first name.
    /// This is used for personalization and identification purposes.
    /// </summary>
    [Required]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's last name.
    /// This complements the first name for complete user identification.
    /// </summary>
    [Required]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's display name.
    /// This is typically derived from email or used for public identification.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}