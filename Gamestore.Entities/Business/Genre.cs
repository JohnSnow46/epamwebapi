namespace Gamestore.Entities.Business;

/// <summary>
/// Represents a game genre entity in the game catalog system.
/// Supports hierarchical genre classification with parent-child relationships,
/// enabling both broad categories and specific sub-genres for comprehensive game categorization.
/// </summary>
public class Genre
{
    /// <summary>
    /// Gets or sets the unique identifier for the genre.
    /// This serves as the primary key for the genre entity.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the genre.
    /// This is the display name shown to users for genre identification and filtering.
    /// Examples include "Action", "RPG", "Strategy", "First-Person Shooter", etc.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier of the parent genre in the hierarchy.
    /// This is nullable to support root-level genres that have no parent.
    /// Enables hierarchical genre structures like "Action" -> "First-Person Shooter".
    /// </summary>
    public Guid? ParentGenreId { get; set; }

    /// <summary>
    /// Gets or sets the parent Genre entity in the hierarchical structure.
    /// This navigation property provides access to the parent genre information
    /// for building genre hierarchies and breadcrumb navigation.
    /// Can be null for top-level genres.
    /// </summary>
    public Genre? ParentGenre { get; set; }

    /// <summary>
    /// Gets or sets the collection of child genres that belong to this genre in the hierarchy.
    /// This navigation property enables access to sub-genres and supports
    /// building complete genre trees for navigation and categorization.
    /// Can be null or empty for leaf-level genres.
    /// </summary>
    public ICollection<Genre>? ChildGenres { get; set; }

    /// <summary>
    /// Gets or sets the collection of GameGenre relationships that associate games with this genre.
    /// This navigation property enables the many-to-many relationship between genres and games,
    /// allowing access to all games categorized under this genre.
    /// Can be null or empty for genres with no associated games.
    /// </summary>
    public ICollection<GameGenre>? GameGenres { get; set; }
}