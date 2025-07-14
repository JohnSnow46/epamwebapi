using Gamestore.Data.Data;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Orders;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repositories;

/// <summary>
/// Repository implementation for managing PaymentTransaction entities in the payment processing system.
/// Provides concrete implementations for payment history tracking, transaction auditing,
/// and payment status management with comprehensive financial operations support.
/// Inherits from the generic Repository pattern and implements IPaymentTransactionRepository interface.
/// </summary>
public class PaymentTransactionRepository(GameCatalogDbContext context) : Repository<PaymentTransaction>(context), IPaymentTransactionRepository
{
    private readonly GameCatalogDbContext _context = context;

    /// <summary>
    /// Retrieves all payment transactions associated with a specific order ordered by processing time.
    /// This method returns the complete payment history for an order, including successful payments,
    /// failed attempts, refunds, and partial payments for comprehensive payment auditing.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to retrieve payment transactions for.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of PaymentTransaction entities
    /// associated with the specified order, ordered by processing time with most recent first.
    /// </returns>
    public async Task<IEnumerable<PaymentTransaction>> GetTransactionsByOrderIdAsync(Guid orderId)
    {
        return await _context.PaymentTransactions
            .Where(pt => pt.OrderId == orderId)
            .OrderByDescending(pt => pt.ProcessedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves the most recent payment transaction for a specific order.
    /// This method provides quick access to the latest payment attempt or status
    /// without loading the entire transaction history, optimized for status checking operations.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to get the latest transaction for.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the most recent PaymentTransaction entity
    /// for the specified order if any transactions exist, or null if no payment transactions have been recorded.
    /// </returns>
    public async Task<PaymentTransaction?> GetLatestTransactionByOrderIdAsync(Guid orderId)
    {
        return await _context.PaymentTransactions
            .Where(pt => pt.OrderId == orderId)
            .OrderByDescending(pt => pt.ProcessedAt)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Retrieves all payment transactions for a specific customer across all their orders.
    /// This method provides comprehensive payment history for customer service, financial reporting,
    /// fraud detection, and customer account management by joining through order relationships.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer whose payment transactions to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of PaymentTransaction entities
    /// for all orders placed by the specified customer, ordered by processing time with most recent first.
    /// </returns>
    public async Task<IEnumerable<PaymentTransaction>> GetTransactionsByCustomerAsync(Guid customerId)
    {
        return await _context.PaymentTransactions
            .Include(pt => pt.Order)
            .Where(pt => pt.Order.CustomerId == customerId)
            .OrderByDescending(pt => pt.ProcessedAt)
            .ToListAsync();
    }
}