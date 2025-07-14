using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.OrdersDto;

/// <summary>
/// Represents a data transfer object for an order in the game store system.
/// Contains order information including customer details, status, and financial summary.
/// </summary>
public class OrderDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the order.
    /// This field is required and represents the order's unique ID in the system.
    /// </summary>
    [Required]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the customer who placed the order.
    /// This field is required and links the order to a specific customer account.
    /// </summary>
    [Required]
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the order was placed.
    /// This field is optional and contains the order creation timestamp.
    /// </summary>
    public DateTime? Date { get; set; }

    /// <summary>
    /// Gets or sets the current status of the order.
    /// Represents the order's state in the fulfillment process (e.g., "Pending", "Processing", "Completed", "Cancelled").
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total monetary amount of the order.
    /// Represents the sum of all items and applicable taxes or fees.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the total number of items in the order.
    /// Represents the count of individual game units purchased.
    /// </summary>
    public int TotalItems { get; set; }
}
