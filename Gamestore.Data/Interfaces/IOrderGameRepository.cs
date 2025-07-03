using Gamestore.Entities.Orders;

namespace Gamestore.Data.Interfaces;
public interface IOrderGameRepository : IRepository<OrderGame>
{
    Task<IEnumerable<OrderGame>> GetOrderGamesByOrderIdAsync(Guid orderId);
    Task<OrderGame?> GetOrderGameAsync(Guid orderId, Guid productId);
    Task<bool> UpdateQuantityAsync(Guid orderGameId, int newQuantity);
    Task<bool> RemoveOrderGameAsync(Guid orderId, Guid productId);
    Task<decimal> GetOrderTotalAsync(Guid orderId);
}