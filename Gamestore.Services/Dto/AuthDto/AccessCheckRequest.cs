using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.AuthDto;

/// <summary>
/// Represents a request to check access permissions for a specific page or resource.
/// Used for authorization validation in the game store system.
/// </summary>
public class AccessCheckRequest
{
    /// <summary>
    /// Gets or sets the target page or resource identifier for access checking.
    /// This indicates which page or section the user is trying to access.
    /// </summary>
    [Required]
    public string TargetPage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional target identifier for resource-specific access checks.
    /// This provides additional context for checking access to specific items or entities.
    /// </summary>
    public string? TargetId { get; set; }
}
