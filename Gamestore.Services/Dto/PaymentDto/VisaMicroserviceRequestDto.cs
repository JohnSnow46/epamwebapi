namespace Gamestore.Services.Dto.PaymentDto;
public class VisaMicroserviceRequestDto
{
    public decimal TransactionAmount { get; set; }
    public string CardHolderName { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public int ExpirationMonth { get; set; }
    public int Cvv { get; set; }
    public int ExpirationYear { get; set; }
}
