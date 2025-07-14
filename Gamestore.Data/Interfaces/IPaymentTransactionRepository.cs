using Gamestore.Entities.Orders;

namespace Gamestore.Data.Interfaces;

/// <summary>
/// Repository interface for managing PaymentTransaction entities in the payment processing system.
/// Provides functionality for tracking payment history, transaction auditing, and payment status management
/// across orders and customers. Supports multiple payment attempts and refund scenarios.
/// Extends the generic repository pattern with payment-specific operations.
/// </summary>
public interface IPaymentTransactionRepository : IRepository<PaymentTransaction>
{
    /// <summary>
    /// Retrieves all payment transactions associated with a specific order.
    /// This method returns the complete payment history for an order, including successful payments,
    /// failed attempts, refunds, and partial payments. Useful for payment auditing and order reconciliation.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to retrieve payment transactions for.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of PaymentTransaction entities
    /// associated with the specified order, ordered chronologically. Returns an empty collection if no transactions exist for the order.
    /// </returns>
    Task<IEnumerable<PaymentTransaction>> GetTransactionsByOrderIdAsync(Guid orderId);

    /// <summary>
    /// Retrieves the most recent payment transaction for a specific order.
    /// This method is particularly useful for determining the current payment status of an order
    /// or getting the latest payment attempt details without loading the entire transaction history.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to get the latest transaction for.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the most recent PaymentTransaction entity
    /// for the specified order if any transactions exist, or null if no payment transactions have been recorded for the order.
    /// </returns>
    Task<PaymentTransaction?> GetLatestTransactionByOrderIdAsync(Guid orderId);

    /// <summary>
    /// Retrieves all payment transactions for a specific customer across all their orders.
    /// This method provides a comprehensive payment history for customer service, financial reporting,
    /// and fraud detection purposes. Includes transactions from all orders placed by the customer.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer whose payment transactions to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of PaymentTransaction entities
    /// for all orders placed by the specified customer. Returns an empty collection if the customer has no payment history.
    /// </returns>
    Task<IEnumerable<PaymentTransaction>> GetTransactionsByCustomerAsync(Guid customerId);
}