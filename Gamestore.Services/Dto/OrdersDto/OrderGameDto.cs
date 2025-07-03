using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.OrdersDto;
public class OrderGameDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public double Price { get; set; }

    [Required]
    public int Quantity { get; set; }

    public int Discount { get; set; }

    public double TotalPrice { get; set; }

    // Additional game info for cart display
    public string? GameName { get; set; }
    public string? GameKey { get; set; }
}
