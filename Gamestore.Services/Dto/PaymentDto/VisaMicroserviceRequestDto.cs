using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.PaymentDto;
public class VisaMicroserviceRequestDto
{
    [Required]
    public string Holder { get; set; } = string.Empty;

    [Required]
    public string CardNumber { get; set; } = string.Empty;

    [Required]
    [Range(1, 12)]
    public int MonthExpire { get; set; }

    [Required]
    [Range(2024, 2099)]
    public int YearExpire { get; set; }

    [Required]
    [Range(100, 9999)]
    public int Cvv2 { get; set; }
}
