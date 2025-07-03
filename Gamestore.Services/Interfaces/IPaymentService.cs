using Gamestore.Services.Dto.PaymentDto;

namespace Gamestore.Services.Interfaces;
public interface IPaymentService
{
    Task<PaymentMethodsResponseDto> GetPaymentMethodsAsync();
    Task<PaymentResponseDto> ProcessPaymentAsync(Guid customerId, PaymentRequestDto paymentRequest);
    Task<PaymentResponseDto> ProcessBankPaymentAsync(Guid customerId);
    Task<PaymentResponseDto> ProcessIBoxPaymentAsync(Guid customerId);
    Task<PaymentResponseDto> ProcessVisaPaymentAsync(Guid customerId, VisaPaymentModelDto visaData);
}
