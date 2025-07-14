namespace Gamestore.Entities.Business;

/// <summary>
/// Represents a game entity in the game catalog system.
/// Contains comprehensive game information including metadata, pricing, inventory,
/// relationships with publishers, genres, and platforms, as well as analytics data.
/// </summary>
public class Game
{
    /// <summary>
    /// Gets or sets the unique identifier for the game.
    /// This serves as the primary key for the game entity.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the unique string key for the game.
    /// This serves as a URL-friendly identifier for web routing and external references.
    /// Should be unique across all games in the system.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the game.
    /// This is the primary title shown to users in catalogs, search results, and game details.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detailed description of the game.
    /// This contains the game's overview, features, storyline, and other descriptive content
    /// displayed on game detail pages and marketing materials.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current price of the game.
    /// This represents the selling price in the system's base currency.
    /// </summary>
    public double Price { get; set; }

    /// <summary>
    /// Gets or sets the number of units available in stock.
    /// This tracks inventory levels for physical or limited digital copies.
    /// Zero indicates the game is out of stock.
    /// </summary>
    public int UnitInStock { get; set; }

    /// <summary>
    /// Gets or sets the discontinued status of the game.
    /// Non-zero values indicate the game has been discontinued and is no longer actively sold.
    /// This allows for soft deletion while maintaining historical data.
    /// </summary>
    public int Discontinued { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the publisher who published this game.
    /// This is a foreign key reference to the Publisher entity and can be null for games without publishers.
    /// </summary>
    public Guid? PublisherId { get; set; }

    /// <summary>
    /// Gets or sets the Publisher entity that published this game.
    /// This navigation property provides access to publisher information including company name and details.
    /// Can be null for games without associated publishers.
    /// </summary>
    public Publisher? Publisher { get; set; }

    /// <summary>
    /// Gets or sets the collection of GameGenre relationships that associate this game with genres.
    /// This navigation property enables the many-to-many relationship between games and genres,
    /// allowing games to be categorized under multiple genres for browsing and filtering.
    /// </summary>
    public ICollection<GameGenre> GameGenres { get; set; }

    /// <summary>
    /// Gets or sets the collection of GamePlatform relationships that associate this game with gaming platforms.
    /// This navigation property enables the many-to-many relationship between games and platforms,
    /// indicating which platforms this game is compatible with or available on.
    /// </summary>
    public ICollection<GamePlatform> GamePlatforms { get; set; }

    /// <summary>
    /// Gets or sets the GameFile entity containing downloadable game files and resources.
    /// This navigation property provides access to game assets, installers, and downloadable content.
    /// Can be null for games that don't have digital files or are physical-only releases.
    /// </summary>
    public GameFile? GameFile { get; set; }

    // filtering/sorting
    /// <summary>
    /// Gets or sets the number of times this game has been viewed by users.
    /// This metric is used for popularity tracking, analytics, and trending algorithms.
    /// Automatically incremented when users view the game details.
    /// </summary>
    public int ViewCount { get; set; }

    /// <summary>
    /// Gets or sets the total number of comments posted for this game.
    /// This count includes all user reviews and discussion comments associated with the game.
    /// Used for community engagement metrics and sorting by discussion activity.
    /// </summary>
    public int CommentCount { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the game was added to the catalog.
    /// This is automatically set to the current UTC time when the game is created,
    /// used for sorting by newest additions and catalog management.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}