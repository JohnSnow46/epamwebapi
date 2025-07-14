using System.Text.Json.Serialization;

namespace Gamestore.Services.Dto.PlatformsDto;

/// <summary>
/// Represents a data transfer object for updating an existing platform with metadata information.
/// Used to modify platform properties in the game store system.
/// </summary>
public class PlatformMetadataUpdateRequestDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the platform to be updated.
    /// This field is required and must match an existing platform in the system.
    /// </summary>
    [JsonPropertyName("id")]
    [JsonRequired]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the new type or name for the platform.
    /// This field is required and should contain the updated platform type (e.g., "PC", "PlayStation 5", "Xbox Series X").
    /// </summary>
    [JsonPropertyName("type")]
    [JsonRequired]
    public string Type { get; set; } = string.Empty;
}
