using Gamestore.Services.Dto.PaymentDto;

namespace Gamestore.Services.Interfaces;
public interface IPaymentMicroserviceClient
{
    Task<bool> ProcessVisaPaymentAsync(VisaMicroserviceRequestDto request);
    Task<bool> ProcessIBoxPaymentAsync(BoxMicroserviceRequestDto request);
}
