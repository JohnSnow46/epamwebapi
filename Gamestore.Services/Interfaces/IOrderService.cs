using Gamestore.Entities.Orders;
using Gamestore.Services.Dto.OrdersDto;

namespace Gamestore.Services.Interfaces;
public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetOrdersAsync(Guid customerId);
    Task<IEnumerable<OrderDto>> GetPaidAndCancelledOrdersAsync(Guid customerId);
    Task<OrderDto?> GetOrderByIdAsync(Guid orderId, Guid requestingCustomerId);
    Task<IEnumerable<OrderGameDto>> GetOrderDetailsAsync(Guid orderId, Guid value);
    Task<Order> CreateOrderAsync(Guid customerId);
    Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus status);
    Task<decimal> GetOrderTotalAsync(Guid orderId);
}
