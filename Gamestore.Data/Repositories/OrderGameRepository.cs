using Gamestore.Data.Data;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Orders;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repositories;
public class OrderGameRepository(GameCatalogDbContext context) : Repository<OrderGame>(context), IOrderGameRepository
{
    private readonly GameCatalogDbContext _context = context;

    public async Task<IEnumerable<OrderGame>> GetOrderGamesByOrderIdAsync(Guid orderId)
    {
        return await _context.OrderGames
            .Include(og => og.Product)
            .Where(og => og.OrderId == orderId)
            .ToListAsync();
    }

    public async Task<OrderGame?> GetOrderGameAsync(Guid orderId, Guid productId)
    {
        return await _context.OrderGames
            .Include(og => og.Product)
            .FirstOrDefaultAsync(og => og.OrderId == orderId && og.ProductId == productId);
    }

    public async Task<bool> UpdateQuantityAsync(Guid orderGameId, int newQuantity)
    {
        var orderGame = await _context.OrderGames.FindAsync(orderGameId);
        if (orderGame == null)
        {
            return false;
        }

        if (newQuantity <= 0)
        {
            _context.OrderGames.Remove(orderGame);
        }
        else
        {
            orderGame.Quantity = newQuantity;
            orderGame.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveOrderGameAsync(Guid orderId, Guid productId)
    {
        var orderGame = await _context.OrderGames
            .FirstOrDefaultAsync(og => og.OrderId == orderId && og.ProductId == productId);

        if (orderGame == null)
        {
            return false;
        }

        _context.OrderGames.Remove(orderGame);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<decimal> GetOrderTotalAsync(Guid orderId)
    {
        return await _context.OrderGames
            .Where(og => og.OrderId == orderId)
            .SumAsync(og => (decimal)(og.Price * og.Quantity * (1 - (og.Discount / 100.0))));
    }
}

