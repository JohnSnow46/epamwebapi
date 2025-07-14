using System.Text.Json.Serialization;

namespace Gamestore.Services.Dto.GenresDto;

/// <summary>
/// Represents a data transfer object for updating an existing genre with metadata information.
/// Used to modify genre properties including name and parent-child relationships in the game store system.
/// </summary>
public class GenreMetadataUpdateRequestDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the genre to be updated.
    /// This field is required and must match an existing genre in the system.
    /// </summary>
    [JsonPropertyName("id")]
    [JsonRequired]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the new name for the genre.
    /// This field is required and should contain the updated genre name.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonRequired]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the parent genre when updating hierarchical relationships.
    /// This field is optional and can be used to change the genre's parent or remove it from a hierarchy.
    /// </summary>
    [JsonPropertyName("parentGenreId")]
    public Guid? ParentGenreId { get; set; }
}
