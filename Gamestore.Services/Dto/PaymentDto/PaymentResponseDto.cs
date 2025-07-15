namespace Gamestore.Services.Dto.PaymentDto;

/// <summary>
/// Represents a payment processing response data transfer object.
/// Contains the result of payment processing operations including status and transaction details.
/// Supports all payment methods defined in README: Bank, IBox terminal, and Visa.
/// </summary>
public class PaymentResponseDto
{
    /// <summary>
    /// Gets or sets a value indicating whether the payment was successful.
    /// This indicates the overall result of the payment processing operation.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the response message from the payment processing.
    /// This provides additional information about the payment result or any errors.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique transaction identifier from the payment processor.
    /// This optional field contains the external transaction reference for successful payments.
    /// </summary>
    public string? TransactionId { get; set; }

    /// <summary>
    /// Gets or sets the payment method used for processing.
    /// This indicates which payment method was used (Bank, IBox terminal, Visa).
    /// </summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Gets or sets the order identifier associated with the payment.
    /// This is required for all payment methods according to README.
    /// </summary>
    public Guid OrderId { get; set; }

    // === IBox Terminal specific properties (from README US7) ===
    /// <summary>
    /// Gets or sets the user identifier who made the payment.
    /// Required for IBox terminal payment response according to README.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the payment was processed.
    /// Required for IBox terminal payment response according to README.
    /// </summary>
    public DateTime? PaymentDate { get; set; }

    /// <summary>
    /// Gets or sets the payment amount.
    /// Required for IBox terminal payment response according to README.
    /// </summary>
    public decimal? Sum { get; set; }

    // === Bank payment specific properties (from README US6) ===
    /// <summary>
    /// Gets or sets the generated invoice file for bank payments.
    /// Contains PDF file data for bank payment method according to README.
    /// </summary>
    public byte[]? InvoiceFile { get; set; }

    /// <summary>
    /// Gets or sets the filename for the invoice file.
    /// Used for bank payment method file downloads.
    /// </summary>
    public string? InvoiceFileName { get; set; }

    /// <summary>
    /// Gets or sets additional data returned from the payment processor.
    /// This optional field can contain provider-specific information or metadata.
    /// </summary>
    public object? Data { get; set; }
}