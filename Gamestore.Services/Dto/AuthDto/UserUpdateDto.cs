using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.AuthDto;

/// <summary>
/// Represents the data transfer object for updating user information.
/// Contains the user fields that can be modified during update operations.
/// </summary>
public class UserUpdateDto
{
    /// <summary>
    /// Gets or sets the user's email address.
    /// This must be a valid email format and serves as a contact method.
    /// </summary>
    [EmailAddress]
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the user's first name.
    /// This is used for personalization and identification purposes.
    /// </summary>
    public string FirstName { get; set; }

    /// <summary>
    /// Gets or sets the user's last name.
    /// This complements the first name for complete user identification.
    /// </summary>
    public string LastName { get; set; }
}
