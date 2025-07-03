using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.CartDto;
public class AddToCartRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; } = 1;
}
