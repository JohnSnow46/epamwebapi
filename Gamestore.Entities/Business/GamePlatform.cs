using System.Text.Json.Serialization;

namespace Gamestore.Entities.Business;

/// <summary>
/// Represents the many-to-many relationship between Game and Platform entities in the game catalog system.
/// This junction table entity enables the association of games with multiple gaming platforms,
/// supporting platform compatibility tracking and platform-based browsing and filtering.
/// </summary>
public class GamePlatform
{
    /// <summary>
    /// Gets or sets the unique identifier for the game-platform relationship.
    /// This serves as the primary key for the junction table entity.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the game in this relationship.
    /// This is a foreign key reference to the Game entity.
    /// </summary>
    public Guid GameId { get; set; }

    /// <summary>
    /// Gets or sets the Game entity that this relationship references.
    /// This navigation property provides access to the complete game information
    /// including metadata, pricing, and other game details.
    /// Excluded from JSON serialization to prevent circular references.
    /// </summary>
    [JsonIgnore]
    public Game Game { get; set; } = null!;

    /// <summary>
    /// Gets or sets the unique identifier of the platform in this relationship.
    /// This is a foreign key reference to the Platform entity.
    /// </summary>
    public Guid PlatformId { get; set; }

    /// <summary>
    /// Gets or sets the Platform entity that this relationship references.
    /// This navigation property provides access to the complete platform information
    /// including platform name, type, and compatibility details.
    /// Excluded from JSON serialization to prevent circular references.
    /// </summary>
    [JsonIgnore]
    public Platform Platform { get; set; } = null!;

    /// <summary>
    /// Gets or sets the timestamp when this game-platform association was created.
    /// This provides audit information for when games were made available on specific platforms,
    /// useful for tracking platform release history and compatibility management.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}