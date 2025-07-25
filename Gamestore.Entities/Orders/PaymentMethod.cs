using System.ComponentModel.DataAnnotations;

namespace Gamestore.Entities.Orders;

/// <summary>
/// Represents a payment method entity available in the game store system.
/// Contains configuration and display information for payment processing options.
/// </summary>
public class PaymentMethod
{
    /// <summary>
    /// Gets or sets the unique identifier for the payment method.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the unique code identifying the payment method.
    /// Used in API requests and internal processing logic.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty; // "bank", "ibox terminal", "visa"

    /// <summary>
    /// Gets or sets the display title or name of the payment method.
    /// This is the primary label shown to customers when selecting payment options.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a detailed description of the payment method.
    /// </summary>
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL to an image or icon representing the payment method.
    /// </summary>
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this payment method is currently active and available.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the display order for sorting payment methods.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the payment method was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when the payment method was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}