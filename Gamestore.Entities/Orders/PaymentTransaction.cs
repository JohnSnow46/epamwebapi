using System.ComponentModel.DataAnnotations;

namespace Gamestore.Entities.Orders;

/// <summary>
/// Represents a payment transaction entity in the game store system.
/// Contains comprehensive payment information including transaction details, status tracking,
/// and relationships with orders for financial record-keeping and order processing.
/// </summary>
public class PaymentTransaction
{
    /// <summary>
    /// Gets or sets the unique identifier for the payment transaction.
    /// This serves as the primary key for the payment transaction entity.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the unique identifier of the associated order.
    /// This establishes the relationship between the payment transaction and its corresponding order.
    /// </summary>
    [Required]
    public Guid OrderId { get; set; }

    /// <summary>
    /// Gets or sets the payment method used for the transaction.
    /// This indicates how the payment was processed (e.g., credit card, PayPal, bank transfer).
    /// </summary>
    [Required]
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the monetary amount of the transaction.
    /// This represents the total amount charged in the system's base currency.
    /// </summary>
    [Required]
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the current processing status of the payment transaction.
    /// This tracks the payment through various stages from initiation to completion or failure.
    /// </summary>
    [Required]
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the external transaction identifier from the payment provider.
    /// This optional field stores the reference ID from third-party payment services
    /// for reconciliation and support purposes.
    /// </summary>
    public string? ExternalTransactionId { get; set; }

    /// <summary>
    /// Gets or sets any error message associated with a failed transaction.
    /// This optional field captures detailed error information when payment processing fails,
    /// providing debugging and customer support information.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the transaction was processed.
    /// This records when the payment transaction was initiated or completed,
    /// defaulting to the current UTC time when the entity is created.
    /// </summary>
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the order associated with this payment transaction.
    /// This navigation property enables the relationship between payment transactions and orders,
    /// allowing access to order details and maintaining referential integrity.
    /// </summary>
    public Order Order { get; set; } = null!;
}
