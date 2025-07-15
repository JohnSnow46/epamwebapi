using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.PaymentDto;

/// <summary>
/// Represents a payment transaction data transfer object.
/// Contains transaction history information for API responses.
/// </summary>
public class PaymentTransactionDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the payment transaction.
    /// </summary>
    [Required]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the associated order.
    /// </summary>
    [Required]
    public Guid OrderId { get; set; }

    /// <summary>
    /// Gets or sets the transaction amount.
    /// </summary>
    [Required]
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the payment method used for this transaction.
    /// </summary>
    [Required]
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current status of the payment transaction.
    /// </summary>
    [Required]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the transaction was processed.
    /// </summary>
    public DateTime ProcessedAt { get; set; }

    /// <summary>
    /// Gets or sets the external transaction identifier from the payment processor.
    /// </summary>
    public string? TransactionId { get; set; }

    /// <summary>
    /// Gets or sets the external transaction identifier (alternative name).
    /// </summary>
    public string? ExternalTransactionId { get; set; }
}