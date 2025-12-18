namespace Gamestore.Services.Dto.OrdersDto;

public class OrderDto
{
    public Guid Id { get; set; }

    public string CustomerId { get; set; }

    public DateTime Date { get; set; }

    public DateTime? ShippedDate { get; set; }
}
