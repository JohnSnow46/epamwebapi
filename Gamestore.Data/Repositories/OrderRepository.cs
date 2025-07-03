using Gamestore.Data.Data;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Orders;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repositories;
public class OrderRepository(GameCatalogDbContext context) : Repository<Order>(context), IOrderRepository
{
    private readonly GameCatalogDbContext _context = context;

    public async Task<Order?> GetOrderWithDetailsAsync(Guid orderId)
    {
        return await _context.Orders
            .Include(o => o.OrderGames)
                .ThenInclude(og => og.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync(Guid customerId)
    {
        return await _context.Orders
            .Include(o => o.OrderGames)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetOrdersByCustomerAndStatusAsync(Guid customerId, OrderStatus status)
    {
        return await _context.Orders
            .Include(o => o.OrderGames)
            .Where(o => o.CustomerId == customerId && o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Order?> GetCartByCustomerAsync(Guid customerId)
    {
        return await _context.Orders
            .Include(o => o.OrderGames)
                .ThenInclude(og => og.Product)
            .FirstOrDefaultAsync(o => o.CustomerId == customerId && o.Status == OrderStatus.Open);
    }

    public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
    {
        return await _context.Orders
            .Include(o => o.OrderGames)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus status)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
        {
            return false;
        }

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;

        if (status is OrderStatus.Paid or OrderStatus.Cancelled)
        {
            order.Date = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public override async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _context.Orders
            .Include(o => o.OrderGames)
                .ThenInclude(og => og.Product)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }
}

