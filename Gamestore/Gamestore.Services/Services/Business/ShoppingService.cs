using Gamestore.Data.Interfaces;
using Gamestore.Entities.Business;
using Gamestore.Services.Dto.OrdersDto;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services.Services.Business;

public class ShoppingService(
    IUnitOfWork unitOfWork,
    ILogger<ShoppingService> logger) : IShoppingService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<ShoppingService> _logger = logger;

    public async Task<IEnumerable<OrderDetailDto>> GetBasketAsync(string customerId)
    {
        _logger.LogInformation("Getting basket for customer: {CustomerId}", customerId);

        try
        {
            var allDetails = await _unitOfWork.OrderDetails.GetAllAsync();

            var basketItems = allDetails
                .Where(d => d.Order.CustomerId == customerId && !d.Order.ShippedDate.HasValue)
                .Select(MapDetailToDto)
                .ToList();

            _logger.LogInformation("Retrieved {Count} items from basket for customer: {CustomerId}", basketItems.Count, customerId);
            return basketItems;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting basket for customer: {CustomerId}", customerId);
            return Enumerable.Empty<OrderDetailDto>();
        }
    }

    public async Task<bool> BuyGameAsync(string gameKey, string customerId)
    {
        _logger.LogInformation("Adding game {GameKey} to basket for customer: {CustomerId}", gameKey, customerId);

        try
        {
            var game = await _unitOfWork.Games.GetKeyAsync(gameKey);
            if (game == null)
            {
                _logger.LogWarning("Game not found: {GameKey}", gameKey);
                return false;
            }

            var orders = await _unitOfWork.Orders.GetAllAsync();
            var cart = orders.FirstOrDefault(o => o.CustomerId == customerId && !o.ShippedDate.HasValue);

            if (cart == null)
            {
                cart = new Order
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customerId,
                    Date = DateTime.UtcNow,
                };
                await _unitOfWork.Orders.AddAsync(cart);
                await _unitOfWork.CompleteAsync();
            }

            var detail = new OrderDetail
            {
                Id = Guid.NewGuid(),
                OrderId = cart.Id,
                ProductId = game.Id,
                Price = game.Price,
                Discount = game.Discontinued,
                Quantity = 1,
            };

            await _unitOfWork.OrderDetails.AddAsync(detail);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("✅ Game {GameKey} added to basket for customer: {CustomerId}", gameKey, customerId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding game {GameKey} to basket for customer: {CustomerId}", gameKey, customerId);
            return false;
        }
    }

    public async Task<bool> CancelGameBuyAsync(string gameKey, string customerId)
    {
        _logger.LogInformation("Removing game {GameKey} from basket for customer: {CustomerId}", gameKey, customerId);

        try
        {
            var game = await _unitOfWork.Games.GetKeyAsync(gameKey);
            if (game == null)
            {
                _logger.LogWarning("Game not found: {GameKey}", gameKey);
                return false;
            }

            var allDetails = await _unitOfWork.OrderDetails.GetAllAsync();
            var detailToRemove = allDetails.FirstOrDefault(d =>
                d.ProductId == game.Id &&
                d.Order.CustomerId == customerId &&
                !d.Order.ShippedDate.HasValue);

            if (detailToRemove == null)
            {
                _logger.LogWarning("Game {GameKey} not found in basket for customer: {CustomerId}", gameKey, customerId);
                return false;
            }

            await _unitOfWork.OrderDetails.DeleteAsync(detailToRemove.Id);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("✅ Game {GameKey} removed from basket for customer: {CustomerId}", gameKey, customerId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing game {GameKey} from basket for customer: {CustomerId}", gameKey, customerId);
            return false;
        }
    }

    private static OrderDetailDto MapDetailToDto(OrderDetail detail) => new()
    {
        Id = detail.Id,
        OrderId = detail.OrderId,
        ProductId = detail.ProductId,
        Price = detail.Price,
        Discount = detail.Discount,
        Quantity = detail.Quantity,
    };
}