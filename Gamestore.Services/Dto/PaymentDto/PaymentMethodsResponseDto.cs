namespace Gamestore.Services.Dto.PaymentDto;

/// <summary>
/// Represents a response containing available payment methods.
/// Used to return a list of payment options to the client.
/// </summary>
public class PaymentMethodsResponseDto
{
    /// <summary>
    /// Gets or sets the list of available payment methods.
    /// Each payment method includes display information and configuration details.
    /// </summary>
    public List<PaymentMethodDto> PaymentMethods { get; set; } = new();
}