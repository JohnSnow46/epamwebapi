using System.Text.Json.Serialization;

namespace Gamestore.Entities.Business;

/// <summary>
/// Represents the many-to-many relationship between Game and Genre entities in the game catalog system.
/// This junction table entity enables the categorization of games under multiple genres,
/// supporting flexible game classification and genre-based browsing and filtering.
/// </summary>
public class GameGenre
{
    /// <summary>
    /// Gets or sets the unique identifier for the game-genre relationship.
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
    /// Gets or sets the unique identifier of the genre in this relationship.
    /// This is a foreign key reference to the Genre entity.
    /// </summary>
    public Guid GenreId { get; set; }

    /// <summary>
    /// Gets or sets the Genre entity that this relationship references.
    /// This navigation property provides access to the complete genre information
    /// including genre name, description, and hierarchical structure.
    /// Excluded from JSON serialization to prevent circular references.
    /// </summary>
    [JsonIgnore]
    public Genre Genre { get; set; } = null!;

    /// <summary>
    /// Gets or sets the timestamp when this game-genre association was created.
    /// This provides audit information for when games were categorized under specific genres,
    /// useful for tracking categorization history and content management.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}