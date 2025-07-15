using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.OrdersDto;

/// <summary>
/// Represents a data transfer object for a game item within an order in the game store system.
/// Contains detailed information about each game product in an order including pricing, quantity, and display information.
/// </summary>
public class OrderGameDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the game product.
    /// This field is required and links to the specific game being ordered.
    /// </summary>
    [Required]
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the unit price of the game at the time of purchase.
    /// This field is required and represents the price per individual game copy.
    /// </summary>
    [Required]
    public double Price { get; set; }

    /// <summary>
    /// Gets or sets the quantity of this game being ordered.
    /// This field is required and represents the number of copies of this game in the order.
    /// </summary>
    [Required]
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the discount percentage applied to this game.
    /// Represents any promotional discount applied to the game price (e.g., 10 for 10% discount).
    /// </summary>
    public int? Discount { get; set; }

    /// <summary>
    /// Gets or sets the total price for this game line item.
    /// Calculated as (Price * Quantity) with any applicable discounts applied.
    /// </summary>
    public double TotalPrice { get; set; }

    /// <summary>
    /// Gets or sets the display name of the game.
    /// This field is optional and used for cart and order display purposes.
    /// </summary>
    public string? GameName { get; set; }

    /// <summary>
    /// Gets or sets the unique key identifier for the game.
    /// This field is optional and provides an alternative identifier for the game.
    /// </summary>
    public string? GameKey { get; set; }
}
