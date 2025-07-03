namespace Gamestore.Services.Dto.PaymentDto;
public class BankInvoiceDto
{
    public Guid UserId { get; set; }
    public Guid OrderId { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime ValidityDate { get; set; }
    public decimal Sum { get; set; }
}
