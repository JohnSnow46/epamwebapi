using System.Text.Json.Serialization;

namespace Gamestore.Entities.Business;

/// <summary>
/// Represents a game publisher entity in the game catalog system.
/// Contains publisher information including company details, description, and web presence,
/// with relationships to the games they have published in the catalog.
/// </summary>
public class Publisher
{
    /// <summary>
    /// Gets or sets the unique identifier for the publisher.
    /// This serves as the primary key for the publisher entity.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the official company name of the publisher.
    /// This is the primary business identifier and display name for the publisher.
    /// Should be unique across all publishers in the system.
    /// Examples include "Electronic Arts", "Ubisoft", "Valve Corporation", etc.
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a detailed description of the publisher.
    /// This contains information about the company's history, focus areas,
    /// notable achievements, and other descriptive content for publisher profiles.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL of the publisher's official website or homepage.
    /// This provides a link to the publisher's main web presence for additional information,
    /// official announcements, and company details.
    /// </summary>
    public string HomePage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of games that this publisher has published.
    /// This navigation property enables the one-to-many relationship between publishers and games,
    /// allowing access to the complete catalog of games from this publisher.
    /// Excluded from JSON serialization to prevent circular references and reduce payload size.
    /// Can be null or empty for publishers with no games in the catalog.
    /// </summary>
    [JsonIgnore]
    public ICollection<Game>? Games { get; set; }
}