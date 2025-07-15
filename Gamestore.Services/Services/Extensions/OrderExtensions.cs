using Gamestore.Entities.Orders;
using Gamestore.Services.Dto.OrdersDto;

namespace Gamestore.Services.Services.Extensions;

/// <summary>
/// Extension methods for Order entity to DTO mapping operations.
/// </summary>
public static class OrderExtensions
{
    /// <summary>
    /// Converts an Order entity to OrderDto.
    /// </summary>
    /// <param name="order">The order entity to convert.</param>
    /// <returns>The corresponding OrderDto.</returns>
    public static OrderDto ToOrderDto(this Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Date = order.Date,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            TotalItems = order.TotalItems
        };
    }

    /// <summary>
    /// Converts a collection of Order entities to OrderDto collection.
    /// </summary>
    /// <param name="orders">The order entities to convert.</param>
    /// <returns>The corresponding OrderDto collection.</returns>
    public static IEnumerable<OrderDto> ToOrderDtos(this IEnumerable<Order> orders)
    {
        return orders.Select(order => order.ToOrderDto());
    }

    /// <summary>
    /// Converts an OrderGame entity to OrderGameDto.
    /// </summary>
    /// <param name="orderGame">The order game entity to convert.</param>
    /// <returns>The corresponding OrderGameDto.</returns>
    public static OrderGameDto ToOrderGameDto(this OrderGame orderGame)
    {
        return new OrderGameDto
        {
            ProductId = orderGame.ProductId,
            Price = orderGame.Price,
            Quantity = orderGame.Quantity,
            Discount = orderGame.Discount,
            TotalPrice = orderGame.TotalPrice,
            GameName = orderGame.Product?.Name,
            GameKey = orderGame.Product?.Key
        };
    }

    /// <summary>
    /// Converts a collection of OrderGame entities to OrderGameDto collection.
    /// </summary>
    /// <param name="orderGames">The order game entities to convert.</param>
    /// <returns>The corresponding OrderGameDto collection.</returns>
    public static IEnumerable<OrderGameDto> ToOrderGameDtos(this IEnumerable<OrderGame> orderGames)
    {
        return orderGames.Select(og => og.ToOrderGameDto());
    }
}
