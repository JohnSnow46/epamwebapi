using System.Text.Json.Serialization;

namespace Gamestore.Services.Dto.GamesDto;

/// <summary>
/// Represents a request data transfer object for updating an existing game.
/// Contains the game identifier and updated information for game modification operations.
/// </summary>
public class GameUpdateRequestDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the game to be updated.
    /// This identifies which game should be modified in the system.
    /// </summary>
    [JsonPropertyName("id")]
    [JsonRequired]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the updated name of the game.
    /// This is the display title shown to users in catalogs and search results.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonRequired]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the updated unique string key for the game.
    /// This serves as a URL-friendly identifier for web routing and external references.
    /// </summary>
    [JsonPropertyName("key")]
    [JsonRequired]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the updated detailed description of the game.
    /// This contains the game's overview, features, and other descriptive content.
    /// </summary>
    [JsonPropertyName("description")]
    [JsonRequired]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the updated price of the game.
    /// This represents the selling price in the system's base currency.
    /// </summary>
    [JsonPropertyName("price")]
    [JsonRequired]
    public double Price { get; set; }

    /// <summary>
    /// Gets or sets the updated number of units available in stock.
    /// This tracks inventory levels for the game.
    /// </summary>
    [JsonPropertyName("unitInStock")]
    [JsonRequired]
    public int UnitInStock { get; set; }

    /// <summary>
    /// Gets or sets the discontinued status of the game.
    /// This indicates whether the game is no longer available for purchase.
    /// </summary>
    [JsonPropertyName("discount")]
    [JsonRequired]
    public int Discontinued { get; set; }
}
