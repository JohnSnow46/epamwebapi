namespace Gamestore.Services.Dto.PaymentDto;
public class IBoxMicroserviceRequestDto
{
    public decimal TransactionAmount { get; set; }
    public Guid AccountNumber { get; set; } // User ID
    public Guid InvoiceNumber { get; set; }  // Order ID
}
