using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.PaymentDto;

/// <summary>
/// Represents a Visa payment model data transfer object.
/// Contains credit card information required for Visa payment processing.
/// </summary>
public class VisaPaymentModelDto
{
    /// <summary>
    /// Gets or sets the cardholder's name as it appears on the credit card.
    /// This is required for payment verification and authorization.
    /// </summary>
    [Required]
    public string Holder { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the credit card number for the payment.
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
    public int MonthExpire { get; set; }

    /// <summary>
    /// Gets or sets the expiration year of the credit card.
    /// Must be a valid year between 2024 and 2099.
    /// </summary>
    [Required]
    [Range(2024, 2099)]
    public int YearExpire { get; set; }

    /// <summary>
    /// Gets or sets the card verification value (CVV2) for security.
    /// Must be a valid 3 or 4 digit code between 100 and 9999.
    /// </summary>
    [Required]
    [Range(100, 9999)]
    public int Cvv2 { get; set; }
}
