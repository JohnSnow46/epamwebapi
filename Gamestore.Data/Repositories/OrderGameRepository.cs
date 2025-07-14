using Gamestore.Data.Data;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Orders;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repositories;

/// <summary>
/// Repository implementation for managing OrderGame entities in the e-commerce order system.
/// Provides concrete implementations for order item management, quantity updates, total calculations,
/// and order-product relationship operations with comprehensive business logic.
/// Inherits from the generic Repository pattern and implements IOrderGameRepository interface.
/// </summary>
public class OrderGameRepository(GameCatalogDbContext context) : Repository<OrderGame>(context), IOrderGameRepository
{
    private readonly GameCatalogDbContext _context = context;

    /// <summary>
    /// Retrieves all OrderGame items for a specific order with product details eagerly loaded.
    /// This method includes product information to provide complete order item details
    /// for order display, cart management, and checkout operations.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to retrieve items for.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of OrderGame entities
    /// with product details loaded for the specified order.
    /// </returns>
    public async Task<IEnumerable<OrderGame>> GetOrderGamesByOrderIdAsync(Guid orderId)
    {
        return await _context.OrderGames
            .Include(og => og.Product)
            .Where(og => og.OrderId == orderId)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves a specific OrderGame item by order and product identifiers with product details loaded.
    /// This method finds the relationship between a specific order and product,
    /// useful for checking item existence, updating quantities, or retrieving item details.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order.</param>
    /// <param name="productId">The unique identifier of the product.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the OrderGame entity
    /// with product details if the relationship exists, or null if not found.
    /// </returns>
    public async Task<OrderGame?> GetOrderGameAsync(Guid orderId, Guid productId)
    {
        return await _context.OrderGames
            .Include(og => og.Product)
            .FirstOrDefaultAsync(og => og.OrderId == orderId && og.ProductId == productId);
    }

    /// <summary>
    /// Updates the quantity of a specific OrderGame item with business logic validation.
    /// If the new quantity is zero or negative, the item is removed from the order.
    /// Otherwise, the quantity is updated and the modification timestamp is set.
    /// </summary>
    /// <param name="orderGameId">The unique identifier of the OrderGame item to update.</param>
    /// <param name="newQuantity">The new quantity value.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is true if the update was successful,
    /// false if the OrderGame item was not found.
    /// </returns>
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

    /// <summary>
    /// Removes a specific product from an order by deleting the OrderGame relationship.
    /// This method finds and removes the order-product association, effectively
    /// removing the item from the customer's cart or order.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to remove the item from.</param>
    /// <param name="productId">The unique identifier of the product to remove.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is true if the item was successfully removed,
    /// false if the item was not found in the order.
    /// </returns>
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

    /// <summary>
    /// Calculates the total monetary value of all items in an order with discount application.
    /// This method computes the sum of (price × quantity × (1 - discount%)) for all order items,
    /// providing the final order total including all applicable discounts.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to calculate the total for.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the total order value
    /// with discounts applied. Returns 0 if the order has no items.
    /// </returns>
    public async Task<decimal> GetOrderTotalAsync(Guid orderId)
    {
        return await _context.OrderGames
            .Where(og => og.OrderId == orderId)
            .SumAsync(og => (decimal)(og.Price * og.Quantity * (1 - (og.Discount / 100.0))));
    }
}