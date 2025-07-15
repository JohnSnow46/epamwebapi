using System.ComponentModel.DataAnnotations;

namespace Gamestore.Entities.Orders;

/// <summary>
/// Represents an order entity in the e-commerce order management system.
/// Manages the complete order lifecycle from cart creation through completion,
/// including customer information, order status tracking, and order item relationships.
/// </summary>
public class Order
{
    /// <summary>
    /// Gets or sets the unique identifier for the order.
    /// This serves as the primary key for the order entity and is automatically generated
    /// when a new order instance is created.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the official date when the order was placed or completed.
    /// This is nullable as it may be set only when the order transitions to certain states
    /// (like Paid or Cancelled). Different from CreatedAt which tracks entity creation.
    /// </summary>
    public DateTime? Date { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the customer who placed this order.
    /// This is required and establishes the relationship between orders and customers
    /// for order history, customer service, and account management.
    /// </summary>
    [Required]
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the current status of the order in its lifecycle.
    /// This is required and tracks the order progression through various states
    /// such as Open (cart), Checkout, Paid, Processing, Shipped, Delivered, or Cancelled.
    /// Defaults to Open status for new orders representing an active shopping cart.
    /// </summary>
    [Required]
    public OrderStatus Status { get; set; } = OrderStatus.Open;

    /// <summary>
    /// Gets or sets the collection of OrderGame entities representing the items in this order.
    /// This navigation property enables the one-to-many relationship between orders and order items,
    /// containing all products, quantities, prices, and discounts for this order.
    /// </summary>
    public ICollection<OrderGame> OrderGames { get; set; } = new List<OrderGame>();

    /// <summary>
    /// Gets or sets the timestamp when the order entity was created.
    /// This is automatically set to the current UTC time when the order is instantiated,
    /// providing audit information for order creation and lifecycle tracking.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when the order was last modified.
    /// This is nullable and should be updated whenever the order or its items are changed,
    /// providing audit trail information for order modifications.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets the total monetary amount of the order including all items, quantities, and discounts.
    /// This computed property calculates the sum of (price × quantity × (1 - discount%)) for all order items,
    /// providing the final order total that would be charged to the customer.
    /// </summary>
    public decimal TotalAmount => OrderGames.Sum(og => (decimal)(og.Price * og.Quantity * (1 - ((og.Discount ?? 0) / 100.0))));

    /// <summary>
    /// Gets the total number of items in the order across all products.
    /// This computed property sums the quantities of all order items,
    /// providing a quick count of total items for display and validation purposes.
    /// </summary>
    public int TotalItems => OrderGames.Sum(og => og.Quantity);
}