namespace Gamestore.Services.Dto.PaymentDto;

/// <summary>
/// Represents a bank invoice data transfer object for payment processing.
/// Contains invoice details for bank transfer payment methods.
/// </summary>
public class BankInvoiceDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the user associated with the invoice.
    /// This identifies the customer who will make the payment.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the order associated with the invoice.
    /// This links the invoice to the specific order being paid for.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Gets or sets the date when the invoice was created.
    /// This timestamp records when the invoice was generated.
    /// </summary>
    public DateTime CreationDate { get; set; }

    /// <summary>
    /// Gets or sets the date until which the invoice is valid.
    /// After this date, the invoice may no longer be accepted for payment.
    /// </summary>
    public DateTime ValidityDate { get; set; }

    /// <summary>
    /// Gets or sets the total amount to be paid according to the invoice.
    /// This represents the monetary value in the system's base currency.
    /// </summary>
    public decimal Sum { get; set; }
}
