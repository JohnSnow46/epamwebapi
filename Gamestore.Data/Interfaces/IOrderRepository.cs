using Gamestore.Entities.Orders;

namespace Gamestore.Data.Interfaces;

/// <summary>
/// Repository interface for managing Order entities.
/// Focuses on data access patterns without business logic validation.
/// </summary>
public interface IOrderRepository : IRepository<Order>
{
    /// <summary>
    /// Retrieves a complete order with all its associated details and related entities.
    /// Repository layer - pure data access without authorization logic.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to retrieve with full details.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the Order entity
    /// with all related details if found, or null if no order with the specified ID exists.
    /// </returns>
    Task<Order?> GetOrderWithDetailsAsync(Guid orderId);

    /// <summary>
    /// Retrieves a complete order with details for a specific customer.
    /// This method combines data filtering with details loading for efficient authorized access.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to retrieve.</param>
    /// <param name="customerId">The customer ID to validate ownership.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the Order entity
    /// if found and belongs to the specified customer, or null if not found or not owned by customer.
    /// </returns>
    Task<Order?> GetOrderWithDetailsByCustomerAsync(Guid orderId, Guid customerId);

    /// <summary>
    /// Retrieves all orders associated with a specific customer.
    /// </summary>
    Task<IEnumerable<Order>> GetOrdersByCustomerAsync(Guid customerId);

    /// <summary>
    /// Retrieves orders for a specific customer filtered by order status.
    /// </summary>
    Task<IEnumerable<Order>> GetOrdersByCustomerAndStatusAsync(Guid customerId, OrderStatus status);

    /// <summary>
    /// Retrieves the active shopping cart for a specific customer.
    /// </summary>
    Task<Order?> GetCartByCustomerAsync(Guid customerId);

    /// <summary>
    /// Retrieves all orders that have a specific status across all customers.
    /// </summary>
    Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);

    /// <summary>
    /// Updates the status of a specific order.
    /// </summary>
    Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus status);
}
