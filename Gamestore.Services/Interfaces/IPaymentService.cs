using Gamestore.Services.Dto.PaymentDto;

namespace Gamestore.Services.Interfaces;
public interface IPaymentService
{
    Task<PaymentMethodsResponseDto> GetAvailablePaymentMethodsAsync(Guid customerId);
    Task<PaymentResponseDto> ProcessPaymentAsync(PaymentRequestDto paymentRequest, Guid customerId);
    Task<IEnumerable<PaymentTransactionDto>> GetPaymentHistoryAsync(Guid customerId);
    Task<PaymentResponseDto> ProcessBankPaymentAsync(Guid customerId);
    Task<PaymentResponseDto> ProcessIBoxPaymentAsync(Guid customerId);
    Task<PaymentResponseDto> ProcessVisaPaymentAsync(Guid customerId, VisaPaymentModelDto visaData);
}
