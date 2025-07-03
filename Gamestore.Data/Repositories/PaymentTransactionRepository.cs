using Gamestore.Data.Data;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Orders;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repositories;
public class PaymentTransactionRepository(GameCatalogDbContext context) : Repository<PaymentTransaction>(context), IPaymentTransactionRepository
{
    private readonly GameCatalogDbContext _context = context;

    public async Task<IEnumerable<PaymentTransaction>> GetTransactionsByOrderIdAsync(Guid orderId)
    {
        return await _context.PaymentTransactions
            .Where(pt => pt.OrderId == orderId)
            .OrderByDescending(pt => pt.ProcessedAt)
            .ToListAsync();
    }

    public async Task<PaymentTransaction?> GetLatestTransactionByOrderIdAsync(Guid orderId)
    {
        return await _context.PaymentTransactions
            .Where(pt => pt.OrderId == orderId)
            .OrderByDescending(pt => pt.ProcessedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<PaymentTransaction>> GetTransactionsByCustomerAsync(Guid customerId)
    {
        return await _context.PaymentTransactions
            .Include(pt => pt.Order)
            .Where(pt => pt.Order.CustomerId == customerId)
            .OrderByDescending(pt => pt.ProcessedAt)
            .ToListAsync();
    }
}
