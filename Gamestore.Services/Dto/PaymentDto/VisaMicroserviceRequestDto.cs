using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.PaymentDto;

/// <summary>
/// Represents a request data transfer object for Visa microservice payment processing.
/// Contains credit card details and transaction information for Visa payment integration.
/// </summary>
public class VisaMicroserviceRequestDto
{
    /// <summary>
    /// Gets or sets the amount to be charged for the transaction.
    /// This represents the monetary value in the system's base currency.
    /// </summary>
    [Required]
    public decimal TransactionAmount { get; set; }

    /// <summary>
    /// Gets or sets the name of the cardholder as it appears on the credit card.
    /// This is required for payment verification and authorization.
    /// </summary>
    [Required]
    public string CardHolderName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the credit card number for the transaction.
    /// This is the primary payment instrument identifier.
    /// </summary>
    [Required]
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expiration month of the credit card.
    /// Must be a valid month between 1 and 12.
    /// </summary>
    [Required]
    [Range(1, 12)]
    public int ExpirationMonth { get; set; }

    /// <summary>
    /// Gets or sets the expiration year of the credit card.
    /// Must be a valid year between 2024 and 2099.
    /// </summary>
    [Required]
    [Range(2024, 2099)]
    public int ExpirationYear { get; set; }

    /// <summary>
    /// Gets or sets the card verification value (CVV) for security.
    /// Must be a valid 3 or 4 digit code between 100 and 9999.
    /// </summary>
    [Required]
    [Range(100, 9999)]
    public int Cvv { get; set; }
}
