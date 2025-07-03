namespace Gamestore.Services.Dto.PaymentDto;
public class PaymentResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public object? Data { get; set; }
}
