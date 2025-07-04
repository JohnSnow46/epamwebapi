using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.PaymentDto;
public class VisaMicroserviceRequestDto
{
    [Required]
    public decimal TransactionAmount { get; set; }

    [Required]
    public string CardHolderName { get; set; } = string.Empty;

    [Required]
    public string CardNumber { get; set; } = string.Empty;

    [Required]
    [Range(1, 12)]
    public int ExpirationMonth { get; set; }

    [Required]
    [Range(2024, 2099)]
    public int ExpirationYear { get; set; }

    [Required]
    [Range(100, 9999)]
    public int Cvv { get; set; }
}
