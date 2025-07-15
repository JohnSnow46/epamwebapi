using Gamestore.Entities.Orders;

namespace Gamestore.Data.Interfaces;

/// <summary>
/// Repository interface for managing OrderGame entities in the e-commerce system.
/// Handles the many-to-many relationships between orders and games, including quantity management,
/// cart operations, and order total calculations.
/// Extends the generic repository pattern with order-specific business logic.
/// </summary>
public interface IOrderGameRepository : IRepository<OrderGame>
{
    /// <summary>
    /// Retrieves all OrderGame items that belong to a specific order.
    /// This method is typically used to display the contents of an order or shopping cart.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to retrieve items for.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of OrderGame entities
    /// associated with the specified order. Returns an empty collection if the order has no items.
    /// </returns>
    Task<IEnumerable<OrderGame>> GetOrderGamesByOrderIdAsync(Guid orderId);

    /// <summary>
    /// Retrieves a specific OrderGame item by order and product identifiers.
    /// This method is useful for checking if a product is already in an order or for updating specific items.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order.</param>
    /// <param name="productId">The unique identifier of the game/product.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the OrderGame entity
    /// if the specified product exists in the order, or null if not found.
    /// </returns>
    Task<OrderGame?> GetOrderGameAsync(Guid orderId, Guid productId);

    /// <summary>
    /// Updates the quantity of a specific OrderGame item.
    /// This method is commonly used for cart quantity adjustments and inventory management.
    /// </summary>
    /// <param name="orderGameId">The unique identifier of the OrderGame item to update.</param>
    /// <param name="newQuantity">The new quantity value. Must be greater than 0.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is true if the update was successful,
    /// false if the OrderGame item was not found or the update failed.
    /// </returns>
    Task<bool> UpdateQuantityAsync(Guid orderGameId, int newQuantity);

    /// <summary>
    /// Removes a specific game/product from an order.
    /// This method is used for cart item removal and order modification operations.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to remove the item from.</param>
    /// <param name="productId">The unique identifier of the game/product to remove.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is true if the item was successfully removed,
    /// false if the item was not found in the order or the removal failed.
    /// </returns>
    Task RemoveOrderGameAsync(Guid orderId, Guid productId);

    /// <summary>
    /// Calculates the total monetary value of all items in an order.
    /// This method considers item prices, quantities, and any applicable discounts to compute the final total.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to calculate the total for.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the total order value as a decimal.
    /// Returns 0 if the order has no items or doesn't exist.
    /// </returns>
    Task<decimal> GetOrderTotalAsync(Guid orderId);
}