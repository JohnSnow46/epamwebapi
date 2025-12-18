namespace Gamestore.Services.Dto.OrdersDto;

public class OrderDetailDto
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public Guid ProductId { get; set; }

    public double Price { get; set; }

    public int Discount { get; set; }

    public int Quantity { get; set; }
}
