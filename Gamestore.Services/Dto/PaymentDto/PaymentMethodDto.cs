namespace Gamestore.Services.Dto.PaymentDto;

/// <summary>
/// Represents a payment method data transfer object for API responses.
/// Contains display information for available payment methods in the system.
/// </summary>
public class PaymentMethodDto
{
    /// <summary>
    /// Gets or sets the URL of the image representing the payment method.
    /// This is used for visual display of the payment method icon or logo.
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title or name of the payment method.
    /// This is the display name shown to users for the payment option.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the payment method.
    /// This provides additional details about the payment option for users.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}