using System.ComponentModel.DataAnnotations;
using Gamestore.Entities.Orders;

namespace Gamestore.Services.Dto.PaymentDto;
public class PaymentTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid OrderId { get; set; }

    [Required]
    public Guid CustomerId { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public string PaymentMethod { get; set; } = string.Empty;

    [Required]
    public PaymentStatus Status { get; set; }

    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

    public string? ExternalTransactionId { get; set; }

    public Order Order { get; set; } = null!;
}
