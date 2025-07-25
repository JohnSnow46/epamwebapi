using Gamestore.Entities.Orders;

namespace Gamestore.Data.Interfaces;

/// <summary>
/// Repository interface for managing PaymentMethod entities.
/// Provides functionality for retrieving active payment methods and validation.
/// </summary>
public interface IPaymentMethodRepository : IRepository<PaymentMethod>
{
    /// <summary>
    /// Retrieves all active payment methods ordered by display order.
    /// </summary>
    Task<IEnumerable<PaymentMethod>> GetActivePaymentMethodsAsync();

    /// <summary>
    /// Retrieves a payment method by its unique code.
    /// </summary>
    Task<PaymentMethod?> GetByCodeAsync(string code);

    /// <summary>
    /// Checks if a payment method with the specified code is supported and active.
    /// </summary>
    Task<bool> IsPaymentMethodSupportedAsync(string code);
}