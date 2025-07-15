using Gamestore.Data.Data;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Orders;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repositories;

/// <summary>
/// Repository implementation for managing Order entities.
/// Focuses on data access patterns without business authorization logic.
/// </summary>
public class OrderRepository(GameCatalogDbContext context) : Repository<Order>(context), IOrderRepository
{
    private readonly GameCatalogDbContext _context = context;

    /// <summary>
    /// Retrieves a complete order with all its associated details and related entities.
    /// Repository layer - pure data access without authorization logic.
    /// </summary>
    public async Task<Order?> GetOrderWithDetailsAsync(Guid orderId)
    {
        return await _context.Orders
            .Include(o => o.OrderGames)
                .ThenInclude(og => og.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    /// <summary>
    /// NEW METHOD: Retrieves order with details for a specific customer.
    /// Data layer handles the filtering, removing the need for service-layer authorization checks.
    /// </summary>
    public async Task<Order?> GetOrderWithDetailsByCustomerAsync(Guid orderId, Guid customerId)
    {
        return await _context.Orders
            .Include(o => o.OrderGames)
                .ThenInclude(og => og.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == customerId);
    }

    /// <summary>
    /// Retrieves all orders associated with a specific customer ordered by creation date.
    /// </summary>
    public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync(Guid customerId)
    {
        return await _context.Orders
            .Include(o => o.OrderGames)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves orders for a specific customer filtered by order status.
    /// </summary>
    public async Task<IEnumerable<Order>> GetOrdersByCustomerAndStatusAsync(Guid customerId, OrderStatus status)
    {
        return await _context.Orders
            .Include(o => o.OrderGames)
            .Where(o => o.CustomerId == customerId && o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves the active shopping cart for a specific customer.
    /// </summary>
    public async Task<Order?> GetCartByCustomerAsync(Guid customerId)
    {
        return await _context.Orders
            .Include(o => o.OrderGames)
                .ThenInclude(og => og.Product)
            .FirstOrDefaultAsync(o => o.CustomerId == customerId &&
                (o.Status == OrderStatus.Open || o.Status == OrderStatus.Checkout));
    }

    /// <summary>
    /// Retrieves all orders that have a specific status across all customers.
    /// </summary>
    public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
    {
        return await _context.Orders
            .Include(o => o.OrderGames)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Updates the status of a specific order.
    /// </summary>
    public async Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus status)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
        {
            return false;
        }

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;

        // Set order date when transitioning to paid or cancelled status
        if ((status == OrderStatus.Paid || status == OrderStatus.Cancelled) && !order.Date.HasValue)
        {
            order.Date = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }
}
