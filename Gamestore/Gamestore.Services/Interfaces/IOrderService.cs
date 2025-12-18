using Gamestore.Services.Dto.OrdersDto;

namespace Gamestore.Services.Interfaces;

public interface IOrderService
{
    Task<OrderDto?> GetOrderAsync(Guid id);

    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();

    Task<OrderDto> CreateOrderAsync(CreateOrderRequest request);

    Task DeleteOrderAsync(Guid id);

    // ==================== ORDER DETAILS OPERATIONS ====================
    Task<IEnumerable<OrderDetailDto>> GetOrderDetailsAsync(Guid orderId);

    Task AddOrderDetailAsync(Guid orderId, AddOrderDetailRequest request);

    Task UpdateOrderDetailAsync(Guid detailId, int quantity);

    Task DeleteOrderDetailAsync(Guid detailId);

    // ==================== SHIPPING & NOTIFICATIONS (EPIC 12 US5) ====================
    Task<ShipOrderResponse> ShipOrderAsync(Guid orderId);

    // ==================== ORDER HISTORY ====================
    Task<IEnumerable<OrderDto>> GetOrderHistoryAsync(string customerId, DateTime from, DateTime to);
}