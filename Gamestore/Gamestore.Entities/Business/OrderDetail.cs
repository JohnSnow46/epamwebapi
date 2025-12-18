namespace Gamestore.Entities.Business;

public class OrderDetail
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrderId { get; set; }

    public Order Order { get; set; } = null!;

    public Guid ProductId { get; set; }

    public Game Game { get; set; } = null!;

    public double Price { get; set; }

    public int Discount { get; set; }

    public int Quantity { get; set; }
}
