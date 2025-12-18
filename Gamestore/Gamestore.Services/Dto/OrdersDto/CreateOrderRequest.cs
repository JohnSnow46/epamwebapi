namespace Gamestore.Services.Dto.OrdersDto;

public class CreateOrderRequest
{
    public string CustomerId { get; set; } = string.Empty;

    public List<Guid> ProductIds { get; set; } = new();
}