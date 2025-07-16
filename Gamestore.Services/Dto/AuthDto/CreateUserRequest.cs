using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.AuthDto;


/// <summary>
/// Represents a request to create a new user account.
/// Contains user information, password, and role assignments for user registration.
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// Gets or sets the user creation data transfer object.
    /// Contains the basic user information needed to create a new account.
    /// </summary>
    [Required]
    public UserCreateDto User { get; set; } = new();

    /// <summary>
    /// Gets or sets the password for the new user account.
    /// This will be hashed and stored securely in the system.
    /// </summary>
    [Required]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of role names to assign to the new user.
    /// These roles determine the user's permissions and access levels in the system.
    /// </summary>
    public List<string> Roles { get; set; } = new();
}