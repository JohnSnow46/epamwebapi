namespace Gamestore.Services.Dto.GamesDto;

/// <summary>
/// Represents a comprehensive request data transfer object for creating a new game with metadata.
/// Contains game information along with publisher, genre, and platform associations.
/// </summary>
public class GameMetadataCreateRequestDto
{
    /// <summary>
    /// Gets or sets the game creation data transfer object.
    /// Contains the basic game information needed to create the game entity.
    /// </summary>
    public GameCreateRequestDto Game { get; set; } = new();

    /// <summary>
    /// Gets or sets the unique identifier of the publisher.
    /// This associates the game with a specific publisher in the system.
    /// </summary>
    public Guid Publisher { get; set; }

    /// <summary>
    /// Gets or sets the optional list of genre identifiers.
    /// These establish many-to-many relationships between the game and genres.
    /// </summary>
    public List<Guid>? Genres { get; set; }

    /// <summary>
    /// Gets or sets the optional list of platform identifiers.
    /// These establish many-to-many relationships between the game and platforms.
    /// </summary>
    public List<Guid>? Platforms { get; set; }
}
