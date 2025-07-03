using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.GamesDto;
public class GameCreateRequestDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    [Required]
    public string? Description { get; set; }

    [Required]
    public double Price { get; set; }

    [Required]
    public int UnitInStock { get; set; }

    [Required]
    public int Discount { get; set; }
}