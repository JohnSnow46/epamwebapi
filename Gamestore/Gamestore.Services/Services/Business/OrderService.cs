using Gamestore.Data.Interfaces;
using Gamestore.Entities.Business;
using Gamestore.Services.Dto.OrdersDto;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services.Services.Business;

public class OrderService(
    IUnitOfWork unitOfWork,
    IOrderNotificationService notificationService,
    ILogger<OrderService> logger) : IOrderService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IOrderNotificationService _notificationService = notificationService;
    private readonly ILogger<OrderService> _logger = logger;

    public async Task<OrderDto?> GetOrderAsync(Guid id)
    {
        _logger.LogInformation("Starting get order operation by ID: {OrderId}", id);

        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            _logger.LogInformation("Order found with ID: {OrderId}", id);
            return MapToDto(order);
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("Order not found with ID: {OrderId}", id);
            return null;
        }
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        _logger.LogInformation("Starting get all orders operation");

        var orders = await _unitOfWork.Orders.GetAllAsync();
        _logger.LogInformation("Retrieved {Count} orders from database", orders.Count());
        return orders.Select(MapToDto);
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request)
    {
        _logger.LogInformation("Starting create order operation for customer: {CustomerId}", request.CustomerId);

        ValidateNotNull(request, nameof(request));
        ValidateString(request.CustomerId, "Customer ID");

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            Date = DateTime.UtcNow,
        };

        await _unitOfWork.Orders.AddAsync(order);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("✅ Order created: {OrderId} for customer: {CustomerId}", order.Id, request.CustomerId);

        return MapToDto(order);
    }

    public async Task DeleteOrderAsync(Guid id)
    {
        _logger.LogInformation("Starting delete order operation for ID: {OrderId}", id);

        ValidateGuid(id, "Order ID");

        await _unitOfWork.Orders.DeleteAsync(id);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Order deleted: {OrderId}", id);
    }

    public async Task<IEnumerable<OrderDetailDto>> GetOrderDetailsAsync(Guid orderId)
    {
        _logger.LogInformation("Starting get order details operation for order: {OrderId}", orderId);

        ValidateGuid(orderId, "Order ID");

        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        var details = order.OrderDetails.Select(MapDetailToDto);

        _logger.LogInformation("Retrieved {Count} order details for order: {OrderId}", details.Count(), orderId);

        return details;
    }

    public async Task AddOrderDetailAsync(Guid orderId, AddOrderDetailRequest request)
    {
        _logger.LogInformation("Starting add order detail operation for order: {OrderId}", orderId);

        ValidateGuid(orderId, "Order ID");
        ValidateNotNull(request, nameof(request));

        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);

        var detail = new OrderDetail
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = request.ProductId,
            Price = request.Price,
            Discount = request.Discount,
            Quantity = request.Quantity,
        };

        order.OrderDetails.Add(detail);
        await _unitOfWork.Orders.UpdateAsync(order);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("✅ Order detail added to order: {OrderId}", orderId);
    }

    public async Task UpdateOrderDetailAsync(Guid detailId, int quantity)
    {
        _logger.LogInformation("Starting update order detail operation for detail: {DetailId}", detailId);

        ValidateGuid(detailId, "Detail ID");

        var allDetails = await _unitOfWork.OrderDetails.GetAllAsync();
        var detail = allDetails.FirstOrDefault(d => d.Id == detailId)
            ?? throw new KeyNotFoundException($"Order detail with ID '{detailId}' not found");

        detail.Quantity = quantity;
        await _unitOfWork.OrderDetails.UpdateAsync(detail);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("✅ Order detail updated: {DetailId}, New quantity: {Quantity}", detailId, quantity);
    }

    public async Task DeleteOrderDetailAsync(Guid detailId)
    {
        _logger.LogInformation("Starting delete order detail operation for detail: {DetailId}", detailId);

        ValidateGuid(detailId, "Detail ID");

        await _unitOfWork.OrderDetails.DeleteAsync(detailId);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Order detail deleted: {DetailId}", detailId);
    }

    public async Task<ShipOrderResponse> ShipOrderAsync(Guid orderId)
    {
        _logger.LogInformation("Starting ship order operation for order: {OrderId}", orderId);

        ValidateGuid(orderId, "Order ID");

        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);

        if (order.ShippedDate.HasValue)
        {
            _logger.LogWarning("Order already shipped: {OrderId}", orderId);
            throw new InvalidOperationException("Order has already been shipped");
        }

        order.ShippedDate = DateTime.UtcNow;
        await _unitOfWork.Orders.UpdateAsync(order);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("✅ Order shipped: {OrderId} at {ShippedDate}", orderId, order.ShippedDate);

        try
        {
            var customer = await _unitOfWork.Users.GetByIdAsync(Guid.Parse(order.CustomerId));
            if (customer != null)
            {
                await _notificationService.NotifyOrderStatusChangedAsync(
                    orderId,
                    "Shipped",
                    customer.Email,
                    customer.FirstName,
                    0m);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send notification for order: {OrderId}", orderId);
        }

        return new ShipOrderResponse
        {
            OrderId = orderId,
            ShippedDate = order.ShippedDate.Value,
            NotificationsSent = true,
        };
    }

    public async Task<IEnumerable<OrderDto>> GetOrderHistoryAsync(string customerId, DateTime from, DateTime to)
    {
        _logger.LogInformation("Starting get order history for customer: {CustomerId}, From: {From}, To: {To}", customerId, from, to);

        ValidateString(customerId, "Customer ID");

        var orders = await _unitOfWork.Orders.GetAllAsync();
        var history = orders
            .Where(o => o.CustomerId == customerId &&
                       o.Date >= from &&
                       o.Date <= to)
            .Select(MapToDto)
            .ToList();

        _logger.LogInformation("Retrieved {Count} orders in history for customer: {CustomerId}", history.Count, customerId);

        return history;
    }

    private static OrderDto MapToDto(Order order) => new()
    {
        Id = order.Id,
        CustomerId = order.CustomerId,
        Date = order.Date,
        ShippedDate = order.ShippedDate,
    };

    private static OrderDetailDto MapDetailToDto(OrderDetail detail) => new()
    {
        Id = detail.Id,
        OrderId = detail.OrderId,
        ProductId = detail.ProductId,
        Price = detail.Price,
        Discount = detail.Discount,
        Quantity = detail.Quantity,
    };

    private static void ValidateNotNull(object? obj, string paramName)
    {
        if (obj == null)
        {
            throw new ArgumentNullException(paramName, $"{paramName} cannot be null");
        }
    }

    private static void ValidateString(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
        }
    }

    private static void ValidateGuid(Guid id, string paramName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException($"{paramName} cannot be empty GUID", paramName);
        }
    }
}