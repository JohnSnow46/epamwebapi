namespace Gamestore.Services.Dto.PaymentDto;

/// <summary>
/// Represents a request data transfer object for IBox microservice payment processing.
/// Contains transaction details required for IBox payment system integration.
/// </summary>
public class BoxMicroserviceRequestDto
{
    /// <summary>
    /// Gets or sets the amount of the transaction to be processed.
    /// This represents the monetary value in the system's base currency.
    /// </summary>
    public decimal TransactionAmount { get; set; }

    /// <summary>
    /// Gets or sets the account number for the transaction.
    /// This corresponds to the User ID in the system.
    /// </summary>
    public Guid AccountNumber { get; set; } // User ID

    /// <summary>
    /// Gets or sets the invoice number for the transaction.
    /// This corresponds to the Order ID in the system.
    /// </summary>
    public Guid InvoiceNumber { get; set; }  // Order ID
}
