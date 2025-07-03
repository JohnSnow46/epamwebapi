using System.ComponentModel.DataAnnotations;

namespace Gamestore.Entities.Orders;
public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime? Date { get; set; }
    [Required]
    public Guid CustomerId { get; set; }
    [Required]
    public OrderStatus Status { get; set; } = OrderStatus.Open;

    public ICollection<OrderGame> OrderGames { get; set; } = new List<OrderGame>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public decimal TotalAmount => OrderGames.Sum(og => (decimal)(og.Price * og.Quantity * (1 - (og.Discount / 100.0))));
    public int TotalItems => OrderGames.Sum(og => og.Quantity);
}
