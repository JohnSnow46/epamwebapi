using System.ComponentModel.DataAnnotations;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Orders;
using Gamestore.Services.Dto.PaymentDto;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services.Services.Payment;
public class PaymentService(
    IUnitOfWork unitOfWork,
    IPaymentMicroserviceClient microserviceClient,
    IPdfGeneratorService pdfGenerator,
    IConfiguration configuration,
    ILogger<PaymentService> logger) : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPaymentMicroserviceClient _microserviceClient = microserviceClient;
    private readonly IPdfGeneratorService _pdfGenerator = pdfGenerator;
    private readonly ILogger<PaymentService> _logger = logger;
    private readonly int _bankInvoiceValidityDays = configuration.GetValue("PaymentSettings:BankInvoiceValidityDays", 30);

    public async Task<PaymentMethodsResponseDto> GetPaymentMethodsAsync()
    {
        _logger.LogInformation("Getting available payment methods");
        var paymentMethods = new List<PaymentMethodDto>
    {
        new()
        {
            Title = "Bank",
            Description = "Pay via bank transfer using generated invoice",
            ImageUrl = "https://cdn-icons-png.flaticon.com/512/8043/8043680.png"
        },
        new()
        {
            Title = "IBox terminal",
            Description = "Pay using IBox terminal service",
            ImageUrl = "https://cdn-icons-png.flaticon.com/512/6008/6008615.png"
        },
        new()
        {
            Title = "Visa",
            Description = "Pay with your Visa credit or debit card",
            ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/4/41/Visa_Logo.png"
        }
    };
        return await Task.FromResult(new PaymentMethodsResponseDto { PaymentMethods = paymentMethods });
    }

    public async Task<PaymentResponseDto> ProcessPaymentAsync(Guid customerId, PaymentRequestDto paymentRequest)
    {
        _logger.LogInformation("Processing payment with method {Method} for customer {CustomerId}",
            paymentRequest.Method, customerId);

        return paymentRequest.Method.ToLowerInvariant() switch
        {
            "bank" => await ProcessBankPaymentAsync(customerId),
            "ibox terminal" => await ProcessIBoxPaymentAsync(customerId),
            "visa" => await ProcessVisaPaymentAsync(customerId, paymentRequest.Model!),
            _ => new PaymentResponseDto
            {
                Success = false,
                Message = $"Unsupported payment method: {paymentRequest.Method}"
            }
        };
    }

    public async Task<PaymentResponseDto> ProcessBankPaymentAsync(Guid customerId)
    {
        _logger.LogInformation("Processing bank payment for customer {CustomerId}", customerId);

        try
        {
            var cart = await GetValidCartAsync(customerId);
            _logger.LogInformation("✅ Cart found for bank payment");

            var orderTotal = await _unitOfWork.OrderGames.GetOrderTotalAsync(cart.Id);
            _logger.LogInformation("✅ Order total calculated: {Total}", orderTotal);

            await _unitOfWork.Orders.UpdateOrderStatusAsync(cart.Id, OrderStatus.Checkout);
            _logger.LogInformation("✅ Order status updated to Checkout");

            var transaction = new PaymentTransaction
            {
                OrderId = cart.Id,
                PaymentMethod = "Bank",
                Amount = orderTotal,
                Status = PaymentStatus.Pending
            };
            await _unitOfWork.PaymentTransactions.AddAsync(transaction);
            _logger.LogInformation("✅ Payment transaction created");

            var invoice = new BankInvoiceDto
            {
                UserId = customerId,
                OrderId = cart.Id,
                CreationDate = DateTime.UtcNow,
                ValidityDate = DateTime.UtcNow.AddDays(_bankInvoiceValidityDays),
                Sum = orderTotal
            };
            _logger.LogInformation("✅ Bank invoice DTO created");

            var pdfBytes = await _pdfGenerator.GenerateBankInvoicePdfAsync(invoice);
            _logger.LogInformation("✅ PDF generated, size: {Size} bytes", pdfBytes.Length);

            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("✅ Database changes committed");

            var responseData = new
            {
                InvoiceFile = Convert.ToBase64String(pdfBytes),
                FileName = $"invoice_{cart.Id}.pdf",
                ValidUntil = invoice.ValidityDate
            };
            _logger.LogInformation("✅ Response data created with InvoiceFile");

            return new PaymentResponseDto
            {
                Success = true,
                Message = "Bank invoice generated successfully",
                Data = responseData
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error processing bank payment for customer {CustomerId}", customerId);
            return new PaymentResponseDto
            {
                Success = false,
                Message = "Failed to process bank payment: " + ex.Message
            };
        }
    }

    public async Task<PaymentResponseDto> ProcessIBoxPaymentAsync(Guid customerId)
    {
        _logger.LogInformation("Processing IBox payment for customer {CustomerId}", customerId);

        try
        {
            var cart = await GetValidCartAsync(customerId);
            var orderTotal = await _unitOfWork.OrderGames.GetOrderTotalAsync(cart.Id);

            await _unitOfWork.Orders.UpdateOrderStatusAsync(cart.Id, OrderStatus.Checkout);

            var transaction = new PaymentTransaction
            {
                OrderId = cart.Id,
                PaymentMethod = "IBox terminal",
                Amount = orderTotal,
                Status = PaymentStatus.Processing
            };
            await _unitOfWork.PaymentTransactions.AddAsync(transaction);

            var microserviceRequest = new BoxMicroserviceRequestDto
            {
                TransactionAmount = orderTotal,
                AccountNumber = customerId,
                InvoiceNumber = cart.Id
            };

            var paymentSuccess = await _microserviceClient.ProcessIBoxPaymentAsync(microserviceRequest);

            if (paymentSuccess)
            {
                transaction.Status = PaymentStatus.Completed;
                await _unitOfWork.Orders.UpdateOrderStatusAsync(cart.Id, OrderStatus.Paid);

                await _unitOfWork.CompleteAsync();

                return new PaymentResponseDto
                {
                    Success = true,
                    Message = "IBox payment processed successfully",
                    Data = new BoxPaymentResponseDto
                    {
                        UserId = customerId,
                        OrderId = cart.Id,
                        PaymentDate = DateTime.UtcNow,
                        Sum = orderTotal
                    }
                };
            }
            else
            {
                transaction.Status = PaymentStatus.Failed;
                await _unitOfWork.Orders.UpdateOrderStatusAsync(cart.Id, OrderStatus.Cancelled);

                await _unitOfWork.CompleteAsync();

                return new PaymentResponseDto
                {
                    Success = false,
                    Message = "IBox payment failed"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing IBox payment for customer {CustomerId}", customerId);
            return new PaymentResponseDto
            {
                Success = false,
                Message = "Failed to process IBox payment: " + ex.Message
            };
        }
    }

    public async Task<PaymentResponseDto> ProcessVisaPaymentAsync(Guid customerId, VisaPaymentModelDto visaData)
    {
        _logger.LogInformation("Processing Visa payment for customer {CustomerId}", customerId);

        try
        {
            var cart = await GetValidCartAsync(customerId);
            var orderTotal = await _unitOfWork.OrderGames.GetOrderTotalAsync(cart.Id);

            // Update order status to Checkout
            await _unitOfWork.Orders.UpdateOrderStatusAsync(cart.Id, OrderStatus.Checkout);

            // Create payment transaction
            var transaction = new PaymentTransaction
            {
                OrderId = cart.Id,
                PaymentMethod = "Visa",
                Amount = orderTotal,
                Status = PaymentStatus.Processing
            };
            await _unitOfWork.PaymentTransactions.AddAsync(transaction);

            var microserviceRequest = new VisaMicroserviceRequestDto
            {
                TransactionAmount = orderTotal,
                CardHolderName = visaData.Holder,
                CardNumber = visaData.CardNumber,
                ExpirationMonth = visaData.MonthExpire,
                ExpirationYear = visaData.YearExpire,
                Cvv = visaData.Cvv2
            };

            var paymentSuccess = await _microserviceClient.ProcessVisaPaymentAsync(microserviceRequest);

            if (paymentSuccess)
            {
                // Update transaction and order status
                transaction.Status = PaymentStatus.Completed;
                await _unitOfWork.Orders.UpdateOrderStatusAsync(cart.Id, OrderStatus.Paid);

                await _unitOfWork.CompleteAsync();

                return new PaymentResponseDto
                {
                    Success = true,
                    Message = "Visa payment processed successfully"
                };
            }
            else
            {
                // Payment failed
                transaction.Status = PaymentStatus.Failed;
                await _unitOfWork.Orders.UpdateOrderStatusAsync(cart.Id, OrderStatus.Cancelled);

                await _unitOfWork.CompleteAsync();

                return new PaymentResponseDto
                {
                    Success = false,
                    Message = "Visa payment failed"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Visa payment for customer {CustomerId}", customerId);
            return new PaymentResponseDto
            {
                Success = false,
                Message = "Failed to process Visa payment: " + ex.Message
            };
        }
    }

    private async Task<Order> GetValidCartAsync(Guid customerId)
    {
        var cart = await _unitOfWork.Orders.GetCartByCustomerAsync(customerId) ?? throw new ValidationException("No active cart found");
        return cart.OrderGames.Count == 0 ? throw new ValidationException("Cart is empty") : cart;
    }
}
