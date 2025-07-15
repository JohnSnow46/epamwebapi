namespace Gamestore.Services.Dto.PaymentDto;

/// <summary>
/// Represents a bank invoice data transfer object for PDF generation.
/// Contains all necessary information to generate a bank payment invoice according to README requirements.
/// </summary>
public class BankInvoiceDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the user/customer.
    /// Required field for bank invoice according to README US6.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the order.
    /// Required field for bank invoice according to README US6.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Gets or sets the creation date of the invoice.
    /// Required field for bank invoice according to README US6.
    /// </summary>
    public DateTime CreationDate { get; set; }

    /// <summary>
    /// Gets or sets the validity date - how long the invoice is valid.
    /// Required field for bank invoice according to README US6.
    /// This should be configurable via application settings.
    /// </summary>
    public DateTime ValidityDate { get; set; }

    /// <summary>
    /// Gets or sets the total amount/sum for the invoice.
    /// Required field for bank invoice according to README US6.
    /// </summary>
    public decimal Sum { get; set; }
}