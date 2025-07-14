namespace Gamestore.Entities.Business;

/// <summary>
/// Represents a game file entity that stores downloadable game content and digital assets.
/// This entity manages the binary data for digital game distributions, installers,
/// patches, and other downloadable game-related files in the catalog system.
/// </summary>
public class GameFile
{
    /// <summary>
    /// Gets or sets the unique identifier for the game file.
    /// This serves as the primary key for the game file entity.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the binary content of the game file.
    /// This contains the actual file data such as game installers, assets, or digital content.
    /// Stored as a byte array to handle various file types and formats.
    /// Defaults to an empty array for new instances.
    /// </summary>
    public byte[] Content { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the unique identifier of the game that this file belongs to.
    /// This is a foreign key reference to the Game entity, establishing a one-to-one relationship.
    /// </summary>
    public Guid GameId { get; set; }

    /// <summary>
    /// Gets or sets the Game entity that this file is associated with.
    /// This navigation property provides access to the complete game information
    /// including metadata, pricing, and other game details.
    /// </summary>
    public Game Game { get; set; } = null!;
}