using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Gamestore.Entities.Auth;

/// <summary>
/// Represents a user entity in the authentication and authorization system.
/// Contains user account information, authentication credentials, profile data,
/// and role relationships for comprehensive user management and access control.
/// </summary>
public class User
{
    /// <summary>
    /// Gets or sets the unique identifier for the user.
    /// This serves as the primary key for the user entity.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user's email address, which serves as the primary login identifier.
    /// This must be a valid email format and is required for user authentication.
    /// Email addresses should be unique across the system.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's first name.
    /// This is required for user identification and personalization purposes.
    /// </summary>
    [Required]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's last name.
    /// This is required for user identification and personalization purposes.
    /// </summary>
    [Required]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hashed password for user authentication.
    /// This field is excluded from JSON serialization for security purposes.
    /// The password should always be stored in hashed form, never as plain text.
    /// </summary>
    [JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the user account is active.
    /// Inactive users cannot log in or access the system.
    /// Defaults to true for new user accounts.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the user's email address has been confirmed.
    /// Email confirmation is typically required for account activation and security verification.
    /// </summary>
    public bool IsEmailConfirmed { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the user account was created.
    /// This is automatically set to the current UTC time when the user is instantiated,
    /// providing audit information for account management.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp of the user's last login.
    /// This is nullable as new users may not have logged in yet.
    /// Used for tracking user activity and security monitoring.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Gets or sets the URL or path to the user's profile picture.
    /// This is optional and can be null if the user hasn't uploaded a profile picture.
    /// </summary>
    public string? ProfilePicture { get; set; }

    // Navigation properties
    /// <summary>
    /// Gets or sets the collection of UserRole relationships that associate this user with roles.
    /// This navigation property enables the many-to-many relationship between users and roles,
    /// allowing this user to be assigned multiple roles for access control.
    /// </summary>
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    // Computed properties
    /// <summary>
    /// Gets the user's full name by combining the first and last names.
    /// This computed property is excluded from JSON serialization to avoid redundancy.
    /// Provides a convenient way to display the user's complete name.
    /// </summary>
    [JsonIgnore]
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Gets a list of role names assigned to this user.
    /// This method extracts the role names from the UserRoles navigation property,
    /// providing a simple way to access the user's role assignments for authorization checks.
    /// </summary>
    /// <returns>A list of role names assigned to this user.</returns>
    public List<string> GetRoleNames()
    {
        return UserRoles.Select(ur => ur.Role.Name).ToList();
    }
}