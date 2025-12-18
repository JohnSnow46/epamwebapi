namespace Gamestore.Services.Dto.OrdersDto;

public class AddOrderDetailRequest
{
    public Guid ProductId { get; set; }

    public int Quantity { get; set; }

    public double Price { get; set; }

    public int Discount { get; set; }
}
