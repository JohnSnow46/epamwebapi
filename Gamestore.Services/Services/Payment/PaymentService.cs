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
                ImageUrl = "/images/payment/bank.png"
            },
            new()
            {
                Title = "IBox terminal",
                Description = "Pay using IBox terminal service",
                ImageUrl = "/images/payment/ibox.png"
            },
            new()
            {
                Title = "Visa",
                Description = "Pay with your Visa credit or debit card",
                ImageUrl = "/images/payment/visa.png"
            }
        };

        return new PaymentMethodsResponseDto { PaymentMethods = paymentMethods };
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
            var orderTotal = await _unitOfWork.OrderGames.GetOrderTotalAsync(cart.Id);

            // Update order status to Checkout
            await _unitOfWork.Orders.UpdateOrderStatusAsync(cart.Id, OrderStatus.Checkout);

            // Create payment transaction
            var transaction = new PaymentTransaction
            {
                OrderId = cart.Id,
                PaymentMethod = "Bank",
                Amount = orderTotal,
                Status = PaymentStatus.Pending
            };
            await _unitOfWork.PaymentTransactions.AddAsync(transaction);

            // Generate bank invoice
            var invoice = new BankInvoiceDto
            {
                UserId = customerId,
                OrderId = cart.Id,
                CreationDate = DateTime.UtcNow,
                ValidityDate = DateTime.UtcNow.AddDays(_bankInvoiceValidityDays),
                Sum = orderTotal
            };

            var pdfBytes = await _pdfGenerator.GenerateBankInvoicePdfAsync(invoice);

            await _unitOfWork.CompleteAsync();

            return new PaymentResponseDto
            {
                Success = true,
                Message = "Bank invoice generated successfully",
                Data = new
                {
                    InvoiceFile = Convert.ToBase64String(pdfBytes),
                    FileName = $"invoice_{cart.Id}.pdf",
                    ValidUntil = invoice.ValidityDate
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing bank payment for customer {CustomerId}", customerId);
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

            // Update order status to Checkout
            await _unitOfWork.Orders.UpdateOrderStatusAsync(cart.Id, OrderStatus.Checkout);

            // Create payment transaction
            var transaction = new PaymentTransaction
            {
                OrderId = cart.Id,
                PaymentMethod = "IBox terminal",
                Amount = orderTotal,
                Status = PaymentStatus.Processing
            };
            await _unitOfWork.PaymentTransactions.AddAsync(transaction);

            // Process with microservice
            var microserviceRequest = new IBoxMicroserviceRequestDto
            {
                TransactionAmount = orderTotal,
                AccountNumber = customerId, // User ID as account number
                InvoiceNumber = cart.Id     // Order ID as invoice number
            };

            var paymentSuccess = await _microserviceClient.ProcessIBoxPaymentAsync(microserviceRequest);

            if (paymentSuccess)
            {
                // Update transaction and order status
                transaction.Status = PaymentStatus.Completed;
                await _unitOfWork.Orders.UpdateOrderStatusAsync(cart.Id, OrderStatus.Paid);

                await _unitOfWork.CompleteAsync();

                return new PaymentResponseDto
                {
                    Success = true,
                    Message = "IBox payment processed successfully",
                    Data = new IBoxPaymentResponseDto
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
                // Payment failed
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

            // Process with microservice
            var microserviceRequest = new VisaMicroserviceRequestDto
            {
                TransactionAmount = orderTotal,              // Oblicz z koszyka
                CardHolderName = visaData.Holder,           // holder -> cardHolderName  
                CardNumber = visaData.CardNumber,           // cardNumber -> cardNumber (bez zmian)
                ExpirationMonth = visaData.MonthExpire,     // monthExpire -> expirationMonth
                ExpirationYear = visaData.YearExpire,       // yearExpire -> expirationYear  
                Cvv = visaData.Cvv2                         // cvv2 -> cvv
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
