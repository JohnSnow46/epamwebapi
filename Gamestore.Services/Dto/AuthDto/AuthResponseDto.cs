namespace Gamestore.Services.Dto.AuthDto;

/// <summary>
/// Represents the response data for authentication operations.
/// Contains user information returned after successful authentication.
/// </summary>
public class AuthResponseDto
{
    /// <summary>
    /// Gets or sets the user's email address.
    /// This serves as the primary identifier for the authenticated user.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's first name.
    /// This is used for personalization and display purposes.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's last name.
    /// This complements the first name for full user identification.
    /// </summary>
    public string LastName { get; set; } = string.Empty;
}
