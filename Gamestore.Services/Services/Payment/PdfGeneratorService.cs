using System.Text;
using Gamestore.Services.Dto.PaymentDto;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services.Services.Payment;
public class PdfGeneratorService(ILogger<PdfGeneratorService> logger) : IPdfGeneratorService
{
    private readonly ILogger<PdfGeneratorService> _logger = logger;

    public async Task<byte[]> GenerateBankInvoicePdfAsync(BankInvoiceDto invoice)
    {
        _logger.LogInformation("Generating bank invoice PDF for order {OrderId}", invoice.OrderId);

        // Simple PDF generation - in a real application, use a proper PDF library like iTextSharp
        var content = GenerateInvoiceContent(invoice);
        var pdfBytes = Encoding.UTF8.GetBytes(content);

        _logger.LogInformation("Generated bank invoice PDF of {Size} bytes", pdfBytes.Length);
        return await Task.FromResult(pdfBytes);
    }

    private static string GenerateInvoiceContent(BankInvoiceDto invoice)
    {
        var sb = new StringBuilder();
        sb.AppendLine("BANK PAYMENT INVOICE");
        sb.AppendLine("===================");
        sb.AppendLine();
        sb.AppendLine($"User ID: {invoice.UserId}");
        sb.AppendLine($"Order ID: {invoice.OrderId}");
        sb.AppendLine($"Creation Date: {invoice.CreationDate:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Valid Until: {invoice.ValidityDate:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Amount: ${invoice.Sum:F2}");
        sb.AppendLine();
        sb.AppendLine("Please use this invoice to complete your bank payment.");
        sb.AppendLine("This invoice is only valid until the specified date.");

        return sb.ToString();
    }
}
