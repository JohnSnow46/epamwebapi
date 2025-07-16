namespace Gamestore.Services.Dto.PaymentDto;

/// <summary>
/// Represents a request data transfer object for IBox payment processing.
/// Contains transaction details required by the IBox payment microservice.
/// </summary>
public class BoxMicroserviceRequestDto
{
    /// <summary>
    /// Gets or sets the amount to be processed in the transaction.
    /// This represents the monetary value in the system's base currency.
    /// </summary>
    public decimal TransactionAmount { get; set; }

    /// <summary>
    /// Gets or sets the account number associated with the transaction.
    /// This identifies the customer account for the payment.
    /// </summary>
    public Guid AccountNumber { get; set; }

    /// <summary>
    /// Gets or sets the invoice number for the transaction.
    /// This links the payment to a specific order in the system.
    /// </summary>
    public Guid InvoiceNumber { get; set; }
}