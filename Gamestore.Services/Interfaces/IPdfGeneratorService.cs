using Gamestore.Services.Dto.PaymentDto;

namespace Gamestore.Services.Interfaces;

/// <summary>
/// Service interface for generating PDF documents.
/// Provides functionality for creating various types of PDF documents including invoices and reports.
/// </summary>
public interface IPdfGeneratorService
{
    /// <summary>
    /// Generates a bank invoice PDF for payment processing.
    /// </summary>
    /// <param name="invoice">The bank invoice data transfer object containing invoice details.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the PDF file as a byte array.</returns>
    Task<byte[]> GenerateBankInvoicePdfAsync(BankInvoiceDto invoice);
}