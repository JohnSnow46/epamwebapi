namespace Gamestore.Entities.Business;

/// <summary>
/// Represents a gaming platform entity in the game catalog system.
/// Defines the various platforms on which games can be played or are compatible with,
/// enabling platform-based filtering, compatibility tracking, and platform-specific features.
/// </summary>
public class Platform
{
    /// <summary>
    /// Gets or sets the unique identifier for the platform.
    /// This serves as the primary key for the platform entity.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the type or name of the gaming platform.
    /// This identifies the specific platform such as "PC", "PlayStation 5", "Xbox Series X",
    /// "Nintendo Switch", "Mobile", "Web Browser", etc.
    /// Used for display purposes and platform compatibility checks.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of GamePlatform relationships that associate games with this platform.
    /// This navigation property enables the many-to-many relationship between platforms and games,
    /// allowing access to all games that are available on or compatible with this platform.
    /// Can be null or empty for platforms with no associated games.
    /// </summary>
    public ICollection<GamePlatform>? GamePlatforms { get; set; }
}