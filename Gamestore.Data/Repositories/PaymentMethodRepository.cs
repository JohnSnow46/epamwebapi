using Gamestore.Data.Data;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Orders;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repositories;

/// <summary>
/// Repository implementation for managing PaymentMethod entities.
/// Provides payment method retrieval and validation operations.
/// </summary>
public class PaymentMethodRepository(GameCatalogDbContext context) : Repository<PaymentMethod>(context), IPaymentMethodRepository
{
    private readonly GameCatalogDbContext _context = context;

    public async Task<IEnumerable<PaymentMethod>> GetActivePaymentMethodsAsync()
    {
        return await _context.PaymentMethods
            .Where(pm => pm.IsActive)
            .OrderBy(pm => pm.DisplayOrder)
            .ToListAsync();
    }

    public async Task<PaymentMethod?> GetByCodeAsync(string code)
    {
        return await _context.PaymentMethods
            .FirstOrDefaultAsync(pm => pm.Code.ToLower() == code.ToLower() && pm.IsActive);
    }

    public async Task<bool> IsPaymentMethodSupportedAsync(string code)
    {
        return await _context.PaymentMethods
            .AnyAsync(pm => pm.Code.ToLower() == code.ToLower() && pm.IsActive);
    }
}