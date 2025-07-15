using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.CartDto;
public class UpdateCartQuantityRequestDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
}
