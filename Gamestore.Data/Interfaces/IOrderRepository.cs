using Gamestore.Entities.Orders;

namespace Gamestore.Data.Interfaces;

/// <summary>
/// Repository interface for managing Order entities in the e-commerce system.
/// Provides comprehensive order management functionality including order lifecycle,
/// customer order history, cart operations, and status management.
/// Extends the generic repository pattern with order-specific business operations.
/// </summary>
public interface IOrderRepository : IRepository<Order>
{
    /// <summary>
    /// Retrieves a complete order with all its associated details and related entities.
    /// This method includes order items, customer information, and other related data
    /// necessary for displaying comprehensive order information.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to retrieve with full details.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the Order entity
    /// with all related details if found, or null if no order with the specified ID exists.
    /// </returns>
    Task<Order?> GetOrderWithDetailsAsync(Guid orderId);

    /// <summary>
    /// Retrieves all orders associated with a specific customer.
    /// This method is used for displaying customer order history and account management.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer whose orders to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of Order entities
    /// belonging to the specified customer. Returns an empty collection if the customer has no orders.
    /// </returns>
    Task<IEnumerable<Order>> GetOrdersByCustomerAsync(Guid customerId);

    /// <summary>
    /// Retrieves orders for a specific customer filtered by order status.
    /// This method enables targeted queries for specific order states like pending, completed, or cancelled orders.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer whose orders to retrieve.</param>
    /// <param name="status">The order status to filter by (e.g., Pending, Completed, Cancelled).</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of Order entities
    /// for the specified customer with the specified status. Returns an empty collection if no matching orders exist.
    /// </returns>
    Task<IEnumerable<Order>> GetOrdersByCustomerAndStatusAsync(Guid customerId, OrderStatus status);

    /// <summary>
    /// Retrieves the active shopping cart for a specific customer.
    /// A cart is represented as an order with "Open" status. If no active cart exists, this indicates
    /// the customer has an empty cart or no pending purchases.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer whose cart to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the Order entity
    /// representing the customer's active cart if one exists, or null if the customer has no active cart.
    /// </returns>
    Task<Order?> GetCartByCustomerAsync(Guid customerId);

    /// <summary>
    /// Retrieves all orders that have a specific status across all customers.
    /// This method is useful for administrative operations, order processing workflows,
    /// and generating status-based reports.
    /// </summary>
    /// <param name="status">The order status to filter by (e.g., Pending, Processing, Shipped).</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of Order entities
    /// with the specified status. Returns an empty collection if no orders have the specified status.
    /// </returns>
    Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);

    /// <summary>
    /// Updates the status of a specific order to reflect its current state in the order lifecycle.
    /// This method is crucial for order processing workflows, shipping updates, and completion tracking.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to update.</param>
    /// <param name="status">The new status to assign to the order (e.g., Processing, Shipped, Delivered).</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is true if the status update was successful,
    /// false if the order was not found or the update failed due to business rules or constraints.
    /// </returns>
    Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus status);
}