using Gamestore.Services.Dto.PaymentDto;

namespace Gamestore.Services.Interfaces;
public interface IPdfGeneratorService
{
    Task<byte[]> GenerateBankInvoicePdfAsync(BankInvoiceDto invoice);
}
