using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Gamestore.Entities.Auth;

/// <summary>
/// Represents a user entity with authentication and profile information.
/// </summary>
public class User
{
    /// <summary>
    /// Gets or sets the unique identifier for the user.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's first name.
    /// </summary>
    [Required]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's last name.
    /// </summary>
    [Required]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hashed password for user authentication.
    /// </summary>
    [JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the user account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the user's email address has been confirmed.
    /// </summary>
    public bool IsEmailConfirmed { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the user account was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp of the user's last login.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Gets or sets the URL or path to the user's profile picture.
    /// </summary>
    public string? ProfilePicture { get; set; }

    // Navigation properties
    /// <summary>
    /// Gets or sets the collection of user-role relationships.
    /// </summary>
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    // Computed properties
    /// <summary>
    /// Gets the user's full name.
    /// </summary>
    [JsonIgnore]
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Gets a list of role names assigned to this user.
    /// </summary>
    /// <returns>A list of role names assigned to this user.</returns>
    public List<string> GetRoleNames()
    {
        return UserRoles.Select(ur => ur.Role.Name).ToList();
    }
}