using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.OrdersDto;
public class OrderDto
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public Guid CustomerId { get; set; }

    public DateTime? Date { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public int TotalItems { get; set; }
}
