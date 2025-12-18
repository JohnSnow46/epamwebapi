using Gamestore.Services.Dto.OrdersDto;

namespace Gamestore.Services.Interfaces;

public interface IShoppingService
{
    Task<IEnumerable<OrderDetailDto>> GetBasketAsync(string customerId);

    Task<bool> BuyGameAsync(string gameKey, string customerId);

    Task<bool> CancelGameBuyAsync(string gameKey, string customerId);
}
