using Gamestore.Services.Dto.OrdersDto;

namespace Gamestore.Services.Interfaces;
public interface ICartService
{
    Task AddGameToCartAsync(string gameKey, Guid customerId, int quantity = 1);
    Task RemoveGameFromCartAsync(string gameKey, Guid customerId);
    Task<IEnumerable<OrderGameDto>> GetCartAsync(Guid customerId);
    Task ClearCartAsync(Guid customerId);
    Task UpdateCartItemQuantityAsync(string gameKey, Guid customerId, int newQuantity);
}
