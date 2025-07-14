using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.CommentsDto;

/// <summary>
/// Represents a data transfer object for creating a user ban request in the game store system.
/// Used to provide information required to ban a user from commenting or other activities.
/// </summary>
public class BanCreateRequestDto
{
    /// <summary>
    /// Gets or sets the username or identifier of the user to be banned.
    /// This field is required and cannot be empty.
    /// </summary>
    [Required]
    public string User { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the duration of the ban period.
    /// This field is required and should specify the ban duration (e.g., "7 days", "1 month", "permanent").
    /// </summary>
    [Required]
    public string Duration { get; set; } = string.Empty;
}
