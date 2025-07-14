using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.AuthDto;

/// <summary>
/// Represents a request to update an existing user account.
/// Contains updated user information, optional role changes, and password updates.
/// </summary>
public class UpdateUserRequest
{
    /// <summary>
    /// Gets or sets the user update data transfer object.
    /// Contains the updated user information to be modified.
    /// </summary>
    [Required]
    public UserUpdateDto User { get; set; } = new();

    /// <summary>
    /// Gets or sets the optional role name to assign to the user.
    /// This allows changing the user's role during the update operation.
    /// </summary>
    public string? RoleName { get; set; }

    /// <summary>
    /// Gets or sets the optional new password for the user.
    /// This allows updating the user's password during the update operation.
    /// </summary>
    public string? Password { get; set; }
}
