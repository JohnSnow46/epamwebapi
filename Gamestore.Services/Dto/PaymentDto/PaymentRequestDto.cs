using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.PaymentDto;
public class PaymentRequestDto
{
    [Required]
    public string Method { get; set; } = string.Empty;

    public VisaPaymentModelDto? Model { get; set; }
}

