using Gamestore.Entities.Orders;

namespace Gamestore.Data.Interfaces;
public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetOrderWithDetailsAsync(Guid orderId);
    Task<IEnumerable<Order>> GetOrdersByCustomerAsync(Guid customerId);
    Task<IEnumerable<Order>> GetOrdersByCustomerAndStatusAsync(Guid customerId, OrderStatus status);
    Task<Order?> GetCartByCustomerAsync(Guid customerId);
    Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);
    Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus status);
}
