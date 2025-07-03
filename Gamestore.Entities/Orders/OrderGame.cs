using System.ComponentModel.DataAnnotations;
using Gamestore.Entities.Business;
using System.Text.Json.Serialization;

namespace Gamestore.Entities.Orders;
public class OrderGame
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid OrderId { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public double Price { get; set; }

    [Required]
    public int Quantity { get; set; }

    public int Discount { get; set; }

    [JsonIgnore]
    public Order Order { get; set; } = null!;

    [JsonIgnore]
    public Game Product { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public double TotalPrice => Price * Quantity * (1 - (Discount / 100.0));
}
