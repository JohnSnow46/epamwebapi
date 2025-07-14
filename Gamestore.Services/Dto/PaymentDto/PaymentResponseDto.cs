namespace Gamestore.Services.Dto.PaymentDto;

/// <summary>
/// Represents a payment processing response data transfer object.
/// Contains the result of payment processing operations including status and transaction details.
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
    /// Gets or sets additional data returned from the payment processor.
    /// This optional field can contain provider-specific information or metadata.
    /// </summary>
    public object? Data { get; set; }
}
