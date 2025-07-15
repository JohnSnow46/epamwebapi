using Gamestore.Data.Interfaces;
using Gamestore.Entities.Orders;
using Gamestore.Services.Dto.OrdersDto;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Gamestore.Services.Services.Extensions;

namespace Gamestore.Services.Services.Orders;

public class OrderService(IUnitOfWork unitOfWork, ILogger<OrderService> logger) : IOrderService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<OrderService> _logger = logger;

    public async Task<IEnumerable<OrderDto>> GetOrdersAsync(Guid customerId)
    {
        _logger.LogInformation("Getting all orders for customer {CustomerId}", customerId);

        var orders = await _unitOfWork.Orders.GetOrdersByCustomerAsync(customerId);
        return orders.ToOrderDtos(); // Using extension method
    }

    public async Task<IEnumerable<OrderDto>> GetPaidAndCancelledOrdersAsync(Guid customerId)
    {
        _logger.LogInformation("Getting paid and cancelled orders for customer {CustomerId}", customerId);

        var paidOrders = await _unitOfWork.Orders.GetOrdersByCustomerAndStatusAsync(customerId, OrderStatus.Paid);
        var cancelledOrders = await _unitOfWork.Orders.GetOrdersByCustomerAndStatusAsync(customerId, OrderStatus.Cancelled);

        var allOrders = paidOrders.Concat(cancelledOrders).OrderByDescending(o => o.CreatedAt);
        return allOrders.ToOrderDtos(); // Using extension method
    }

    /// <summary>
    /// UPDATED: Service delegates user validation to repository layer via specific method.
    /// </summary>
    public async Task<OrderDto?> GetOrderByIdAsync(Guid orderId, Guid requestingCustomerId)
    {
        _logger.LogInformation("Getting order by ID {OrderId} for customer {CustomerId}", orderId, requestingCustomerId);

        // Repository handles the customer filtering - no business logic in service
        var order = await _unitOfWork.Orders.GetOrderWithDetailsByCustomerAsync(orderId, requestingCustomerId);

        return order?.ToOrderDto(); // Using extension method, null-safe
    }

    /// <summary>
    /// UPDATED: Service delegates user validation to repository layer.
    /// </summary>
    public async Task<IEnumerable<OrderGameDto>> GetOrderDetailsAsync(Guid orderId, Guid requestingCustomerId)
    {
        _logger.LogInformation("Getting order details for order {OrderId} by customer {CustomerId}", orderId, requestingCustomerId);

        // Repository handles the customer filtering - no authorization logic in service
        var order = await _unitOfWork.Orders.GetOrderWithDetailsByCustomerAsync(orderId, requestingCustomerId) ?? throw new KeyNotFoundException($"Order with ID '{orderId}' not found or access denied");
        var orderGames = await _unitOfWork.OrderGames.GetOrderGamesByOrderIdAsync(orderId);
        return orderGames.Select(og => new OrderGameDto
        {
            ProductId = og.ProductId,
            Price = og.Price,
            Quantity = og.Quantity,
            Discount = og.Discount,
            TotalPrice = og.TotalPrice,
            GameName = og.Product?.Name,
            GameKey = og.Product?.Key
        });
    }

    public async Task<Order> CreateOrderAsync(Guid customerId)
    {
        _logger.LogInformation("Creating new order for customer {CustomerId}", customerId);

        var order = new Order
        {
            CustomerId = customerId,
            Status = OrderStatus.Open
        };

        await _unitOfWork.Orders.AddAsync(order);
        await _unitOfWork.CompleteAsync();

        return order;
    }

    public async Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus status)
    {
        _logger.LogInformation("Updating order {OrderId} status to {Status}", orderId, status);

        var result = await _unitOfWork.Orders.UpdateOrderStatusAsync(orderId, status);
        if (result)
        {
            await _unitOfWork.CompleteAsync();
        }

        return result;
    }

    public async Task<decimal> GetOrderTotalAsync(Guid orderId)
    {
        return await _unitOfWork.OrderGames.GetOrderTotalAsync(orderId);
    }
}