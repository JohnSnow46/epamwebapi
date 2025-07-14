namespace Gamestore.Services.Dto.PlatformsDto;

/// <summary>
/// Represents a data transfer object for creating a platform with metadata information.
/// Used to provide both platform creation data and additional metadata context in the game store system.
/// </summary>
public class PlatformMetadataCreateRequestDto
{
    /// <summary>
    /// Gets or sets the platform creation request containing the main platform information.
    /// This includes the platform type and other creation details.
    /// </summary>
    public PlatformCreateRequestDto Platform { get; set; } = new PlatformCreateRequestDto();
}
