using Gamestore.Data.Data;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Orders;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repositories;

/// <summary>
/// Repository implementation for managing Order entities in the e-commerce order management system.
/// Provides concrete implementations for order lifecycle operations, customer order management,
/// cart functionality, and order status tracking with comprehensive business logic.
/// Inherits from the generic Repository pattern and implements IOrderRepository interface.
/// </summary>
public class OrderRepository(GameCatalogDbContext context) : Repository<Order>(context), IOrderRepository
{
    private readonly GameCatalogDbContext _context = context;

    /// <summary>
    /// Retrieves a complete order with all its associated details and related entities.
    /// This method eagerly loads order items and product information to provide
    /// comprehensive order information for detailed order views and processing.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to retrieve with full details.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the Order entity
    /// with all related details if found, or null if no order with the specified ID exists.
    /// </returns>
    public async Task<Order?> GetOrderWithDetailsAsync(Guid orderId)
    {
        return await _context.Orders
            .Include(o => o.OrderGames)
                .ThenInclude(og => og.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    /// <summary>
    /// Retrieves all orders associated with a specific customer ordered by creation date.
    /// This method provides the complete order history for a customer with order items included,
    /// useful for account management and customer service operations.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer whose orders to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of Order entities
    /// belonging to the specified customer, ordered chronologically with most recent first.
    /// </returns>
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
    /// This method enables targeted queries for specific order states with order items included,
    /// useful for displaying pending orders, completed purchases, or cancelled orders.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer whose orders to retrieve.</param>
    /// <param name="status">The order status to filter by.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of Order entities
    /// for the specified customer with the specified status, ordered chronologically.
    /// </returns>
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
    /// A cart is represented as an order with "Open" or "Checkout" status. This method finds
    /// the customer's current shopping session for cart management operations.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer whose cart to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the Order entity
    /// representing the customer's active cart if one exists, or null if no active cart exists.
    /// </returns>
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
    /// This method is useful for administrative operations, order processing workflows,
    /// and generating status-based reports for business operations.
    /// </summary>
    /// <param name="status">The order status to filter by.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of Order entities
    /// with the specified status, ordered by creation date with most recent first.
    /// </returns>
    public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
    {
        return await _context.Orders
            .Include(o => o.OrderGames)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Updates the status of a specific order to reflect its current state in the order lifecycle.
    /// This method handles order progression through various states, updates timestamps,
    /// and sets the order date for final states (Paid or Cancelled).
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to update.</param>
    /// <param name="status">The new status to assign to the order.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is true if the status update was successful,
    /// false if the order was not found.
    /// </returns>
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

    /// <summary>
    /// Retrieves all orders from the database with all related entities eagerly loaded.
    /// This method overrides the base implementation to include comprehensive order data
    /// including order items and product information for complete order management display.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of all Order entities
    /// with their complete related data loaded, ordered chronologically with most recent first.
    /// </returns>
    public override async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _context.Orders
            .Include(o => o.OrderGames)
                .ThenInclude(og => og.Product)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }
}