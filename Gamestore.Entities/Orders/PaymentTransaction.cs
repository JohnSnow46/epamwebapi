using System.ComponentModel.DataAnnotations;

namespace Gamestore.Entities.Orders;
public class PaymentTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid OrderId { get; set; }

    [Required]
    public string PaymentMethod { get; set; } = string.Empty;

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public PaymentStatus Status { get; set; }

    public string? ExternalTransactionId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Order Order { get; set; } = null!;
}
