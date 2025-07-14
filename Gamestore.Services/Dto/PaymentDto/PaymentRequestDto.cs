using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.PaymentDto;

/// <summary>
/// Represents a payment request data transfer object.
/// Contains payment method information and optional payment model details.
/// </summary>
public class PaymentRequestDto
{
    /// <summary>
    /// Gets or sets the payment method identifier.
    /// This specifies which payment method should be used for processing.
    /// </summary>
    [Required]
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional Visa payment model.
    /// This contains credit card details when using Visa payment method.
    /// </summary>
    public VisaPaymentModelDto? Model { get; set; }
}

