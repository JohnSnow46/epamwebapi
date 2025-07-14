using System.Text.Json.Serialization;

namespace Gamestore.Services.Dto.GamesDto;

/// <summary>
/// Represents a comprehensive request data transfer object for updating a game with metadata.
/// Contains updated game information along with publisher, genre, and platform associations.
/// </summary>
public class GameMetadataUpdateRequestDto
{
    /// <summary>
    /// Gets or sets the game update data transfer object.
    /// Contains the updated game information to be modified.
    /// </summary>
    [JsonPropertyName("game")]
    public GameUpdateRequestDto Game { get; set; } = new();

    /// <summary>
    /// Gets or sets the unique identifier of the publisher.
    /// This associates the game with a specific publisher in the system.
    /// </summary>
    [JsonPropertyName("publisher")]
    [JsonRequired]
    public Guid Publisher { get; set; }

    /// <summary>
    /// Gets or sets the optional list of genre identifiers.
    /// These establish many-to-many relationships between the game and genres.
    /// </summary>
    [JsonPropertyName("genres")]
    public List<Guid>? Genres { get; set; }

    /// <summary>
    /// Gets or sets the optional list of platform identifiers.
    /// These establish many-to-many relationships between the game and platforms.
    /// </summary>
    [JsonPropertyName("platforms")]
    public List<Guid>? Platforms { get; set; }
}
