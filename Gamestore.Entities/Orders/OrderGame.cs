using System.ComponentModel.DataAnnotations;
using Gamestore.Entities.Business;
using System.Text.Json.Serialization;

namespace Gamestore.Entities.Orders;

/// <summary>
/// Represents an order item entity that manages the many-to-many relationship between orders and games.
/// Contains detailed information about specific products within an order including pricing,
/// quantities, discounts, and timestamps for comprehensive order item management.
/// </summary>
public class OrderGame
{
    /// <summary>
    /// Gets or sets the unique identifier for the order item.
    /// This serves as the primary key for the order-game relationship entity
    /// and is automatically generated when a new order item is created.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the unique identifier of the order that contains this item.
    /// This is required and establishes the foreign key relationship to the Order entity.
    /// </summary>
    [Required]
    public Guid OrderId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the game/product in this order item.
    /// This is required and establishes the foreign key relationship to the Game entity.
    /// </summary>
    [Required]
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the price of the product at the time it was added to the order.
    /// This is required and captures the historical price to maintain order integrity
    /// even if the current product price changes after the order is placed.
    /// </summary>
    [Required]
    public double Price { get; set; }

    /// <summary>
    /// Gets or sets the quantity of this product ordered by the customer.
    /// This is required and must be a positive integer representing the number
    /// of units of this product included in the order.
    /// </summary>
    [Required]
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the discount percentage applied to this order item.
    /// This represents the discount as a percentage (0-100) and is used in
    /// total price calculations. A value of 0 means no discount is applied.
    /// </summary>
    public int Discount { get; set; }

    /// <summary>
    /// Gets or sets the Order entity that this item belongs to.
    /// This navigation property provides access to the complete order information
    /// including customer details, order status, and other order items.
    /// Excluded from JSON serialization to prevent circular references.
    /// </summary>
    [JsonIgnore]
    public Order Order { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Game entity representing the product in this order item.
    /// This navigation property provides access to the complete game information
    /// including name, description, publisher, and other game details.
    /// Excluded from JSON serialization to prevent circular references.
    /// </summary>
    [JsonIgnore]
    public Game Product { get; set; } = null!;

    /// <summary>
    /// Gets or sets the timestamp when this order item was created.
    /// This is automatically set to the current UTC time when the order item is instantiated,
    /// providing audit information for when items were added to orders.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when this order item was last modified.
    /// This is nullable and should be updated whenever the item details are changed,
    /// such as quantity updates or discount applications.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets the total price for this order item including quantity and discount calculations.
    /// This computed property calculates: Price × Quantity × (1 - Discount%),
    /// providing the final amount for this specific item in the order.
    /// </summary>
    public double TotalPrice => Price * Quantity * (1 - (Discount / 100.0));
}