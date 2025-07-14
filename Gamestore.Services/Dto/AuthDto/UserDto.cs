namespace Gamestore.Services.Dto.AuthDto;

/// <summary>
/// Represents a user data transfer object for API responses.
/// Contains comprehensive user information including profile data, activity status, and role assignments.
/// </summary>
public class UserDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the user.
    /// This serves as the primary key for the user entity.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's display name.
    /// This is the primary identifier used for the user in the system.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's first name.
    /// This is used for personalization and formal identification.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's last name.
    /// This complements the first name for full user identification.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the user account is active.
    /// Inactive users cannot log in or access the system.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the user account was created.
    /// This provides audit information for user registration.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the user's last login.
    /// This optional field tracks user activity and engagement.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Gets or sets the list of role names assigned to the user.
    /// These roles determine the user's permissions and access levels in the system.
    /// </summary>
    public List<string> Roles { get; set; } = new();
}