namespace Gamestore.Services.Dto.PaymentDto;

/// <summary>
/// Represents a response data transfer object from IBox payment processing.
/// Contains payment confirmation details returned by the IBox microservice.
/// </summary>
public class BoxPaymentResponseDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the user who made the payment.
    /// This identifies the customer associated with the payment transaction.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the order that was paid for.
    /// This links the payment to the specific order in the system.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the payment was processed.
    /// This timestamp confirms when the payment was completed.
    /// </summary>
    public DateTime PaymentDate { get; set; }

    /// <summary>
    /// Gets or sets the amount that was successfully paid.
    /// This represents the monetary value in the system's base currency.
    /// </summary>
    public decimal Sum { get; set; }
}
