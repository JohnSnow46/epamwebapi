namespace Gamestore.Entities.Business;

/// <summary>
/// Represents a game entity in the catalog.
/// </summary>
public class Game
{
    /// <summary>
    /// Gets or sets the unique identifier for the game.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the unique string key for the game.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the game.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detailed description of the game.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current price of the game.
    /// </summary>
    public double Price { get; set; }

    /// <summary>
    /// Gets or sets the number of units available in stock.
    /// </summary>
    public int UnitInStock { get; set; }

    /// <summary>
    /// Gets or sets the discontinued status of the game.
    /// </summary>
    public int Discontinued { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the publisher.
    /// </summary>
    public Guid? PublisherId { get; set; }

    /// <summary>
    /// Gets or sets the publisher navigation property.
    /// </summary>
    public Publisher? Publisher { get; set; }

    /// <summary>
    /// Gets or sets the collection of game-genre relationships.
    /// </summary>
    public ICollection<GameGenre> GameGenres { get; set; }

    /// <summary>
    /// Gets or sets the collection of game-platform relationships.
    /// </summary>
    public ICollection<GamePlatform> GamePlatforms { get; set; }

    /// <summary>
    /// Gets or sets the game file navigation property.
    /// </summary>
    public GameFile? GameFile { get; set; }

    /// <summary>
    /// Gets or sets the number of times this game has been viewed.
    /// </summary>
    public int ViewCount { get; set; }

    /// <summary>
    /// Gets or sets the total number of comments for this game.
    /// </summary>
    public int CommentCount { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the game was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}