using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.GamesDto;

/// <summary>
/// Represents a request data transfer object for creating a new game.
/// Contains the basic game information required for game creation operations.
/// </summary>
public class GameCreateRequestDto
{
    /// <summary>
    /// Gets or sets the name of the game.
    /// This is the display title shown to users in catalogs and search results.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique string key for the game.
    /// This serves as a URL-friendly identifier for web routing and external references.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detailed description of the game.
    /// This contains the game's overview, features, and other descriptive content.
    /// </summary>
    [Required]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the price of the game.
    /// This represents the selling price in the system's base currency.
    /// </summary>
    [Required]
    public double Price { get; set; }

    /// <summary>
    /// Gets or sets the number of units available in stock.
    /// This tracks inventory levels for the game.
    /// </summary>
    [Required]
    public int UnitInStock { get; set; }

    /// <summary>
    /// Gets or sets the discount percentage for the game.
    /// This represents the discount applied to the base price.
    /// </summary>
    [Required]
    public int Discount { get; set; }
}