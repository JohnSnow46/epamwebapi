using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.CartDto;

/// <summary>
/// Represents a data transfer object for adding items to the shopping cart in the game store system.
/// Used to specify the quantity of a game to be added to the user's cart.
/// </summary>
public class AddToCartRequestDto
{
    /// <summary>
    /// Gets or sets the quantity of the game to add to the cart.
    /// This field must be at least 1 and defaults to 1. Represents the number of copies of the game to purchase.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; } = 1;
}
