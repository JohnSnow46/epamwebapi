namespace Gamestore.Entities.Orders;

/// <summary>
/// Represents a payment method option available to customers during the checkout process.
/// Contains display information for payment method selection including visual elements,
/// descriptions, and identification details for various payment processing options.
/// </summary>
public class PaymentMethod
{
    /// <summary>
    /// Gets or sets the display title or name of the payment method.
    /// This is the primary label shown to customers when selecting payment options.
    /// Examples include "Credit Card", "PayPal", "Bank Transfer", "Apple Pay", etc.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a detailed description of the payment method.
    /// This provides additional information about the payment option, processing time,
    /// fees, requirements, or other relevant details to help customers make informed choices.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL to an image or icon representing the payment method.
    /// This visual element helps customers quickly identify and select their preferred
    /// payment option through recognizable logos, icons, or branding elements.
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;
}