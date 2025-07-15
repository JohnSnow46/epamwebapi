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

    public async Task<PaymentMethodsResponseDto> GetAvailablePaymentMethodsAsync(Guid customerId)
    {
        _logger.LogInformation("Getting available payment methods for customer {CustomerId}", customerId);

        // Service layer handles customer validation
        var customer = await ValidateCustomerAsync(customerId);

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

        return new PaymentMethodsResponseDto { PaymentMethods = paymentMethods };
    }

    public async Task<PaymentResponseDto> ProcessPaymentAsync(PaymentRequestDto paymentRequest, Guid customerId)
    {
        _logger.LogInformation("Processing payment with method {Method} for customer {CustomerId}",
            paymentRequest.Method, customerId);

        // Service layer handles customer and cart validation
        await ValidateCustomerAsync(customerId);
        await ValidateCustomerCartAsync(customerId);

        return paymentRequest.Method.ToLowerInvariant() switch
        {
            "bank" => await ProcessBankPaymentAsync(customerId),
            "ibox terminal" => await ProcessIBoxPaymentAsync(customerId),
            "visa" => await ProcessVisaPaymentAsync(customerId, paymentRequest.Model!),
            _ => throw new ValidationException($"Unsupported payment method: {paymentRequest.Method}")
        };
    }

    public async Task<IEnumerable<PaymentTransactionDto>> GetPaymentHistoryAsync(Guid customerId)
    {
        _logger.LogInformation("Getting payment history for customer {CustomerId}", customerId);

        // Service layer handles customer validation
        await ValidateCustomerAsync(customerId);

        var transactions = await _unitOfWork.PaymentTransactions.GetTransactionsByCustomerAsync(customerId);

        return transactions.Select(t => new PaymentTransactionDto
        {
            Id = t.Id,
            OrderId = t.OrderId,
            Amount = t.Amount,
            PaymentMethod = t.PaymentMethod,
            Status = t.Status.ToString(),
            ProcessedAt = t.ProcessedAt,
            TransactionId = t.ExternalTransactionId
        });
    }

    public async Task<PaymentResponseDto> ProcessBankPaymentAsync(Guid customerId)
    {
        _logger.LogInformation("Processing bank payment for customer {CustomerId}", customerId);

        var cart = await GetCustomerCartAsync(customerId);
        var orderTotal = await _unitOfWork.OrderGames.GetOrderTotalAsync(cart.Id);

        // Generate bank invoice PDF
        var invoiceData = new
        {
            OrderId = cart.Id,
            CustomerId = customerId,
            Amount = orderTotal,
            DueDate = DateTime.UtcNow.AddDays(_bankInvoiceValidityDays),
            Items = cart.OrderGames.Select(og => new
            {
                Name = og.Product?.Name,
                Quantity = og.Quantity,
                Price = og.Price,
                Total = og.TotalPrice
            })
        };

        var invoicePdf = await _pdfGenerator.GenerateInvoiceAsync(invoiceData);

        // Update cart to checkout status
        await _unitOfWork.Orders.UpdateOrderStatusAsync(cart.Id, OrderStatus.Checkout);
        await _unitOfWork.CompleteAsync();

        return new PaymentResponseDto
        {
            Success = true,
            OrderId = cart.Id,
            PaymentMethod = "Bank",
            InvoiceFile = invoicePdf,
            Message = "Bank invoice generated successfully. Please complete payment within 30 days."
        };
    }

    public async Task<PaymentResponseDto> ProcessIBoxPaymentAsync(Guid customerId)
    {
        _logger.LogInformation("Processing IBox payment for customer {CustomerId}", customerId);

        var cart = await GetCustomerCartAsync(customerId);
        var orderTotal = await _unitOfWork.OrderGames.GetOrderTotalAsync(cart.Id);

        var iboxRequest = new BoxMicroserviceRequestDto
        {
            OrderId = cart.Id,
            Amount = orderTotal,
            CustomerId = customerId
        };

        var success = await _microserviceClient.ProcessIBoxPaymentAsync(iboxRequest);

        if (success)
        {
            await _unitOfWork.Orders.UpdateOrderStatusAsync(cart.Id, OrderStatus.Paid);

            // Create payment transaction record
            await CreatePaymentTransactionAsync(cart.Id, orderTotal, "IBox Terminal", customerId);

            await _unitOfWork.CompleteAsync();

            return new PaymentResponseDto
            {
                Success = true,
                OrderId = cart.Id,
                PaymentMethod = "IBox Terminal",
                TransactionId = Guid.NewGuid().ToString(),
                Message = "Payment processed successfully via IBox Terminal"
            };
        }

        throw new InvalidOperationException("IBox payment processing failed");
    }

    public async Task<PaymentResponseDto> ProcessVisaPaymentAsync(Guid customerId, VisaPaymentModelDto visaData)
    {
        _logger.LogInformation("Processing Visa payment for customer {CustomerId}", customerId);

        var cart = await GetCustomerCartAsync(customerId);
        var orderTotal = await _unitOfWork.OrderGames.GetOrderTotalAsync(cart.Id);

        var visaRequest = new VisaMicroserviceRequestDto
        {
            OrderId = cart.Id,
            Amount = orderTotal,
            CustomerId = customerId,
            CardData = visaData
        };

        var success = await _microserviceClient.ProcessVisaPaymentAsync(visaRequest);

        if (success)
        {
            await _unitOfWork.Orders.UpdateOrderStatusAsync(cart.Id, OrderStatus.Paid);

            // Create payment transaction record
            await CreatePaymentTransactionAsync(cart.Id, orderTotal, "Visa", customerId);

            await _unitOfWork.CompleteAsync();

            return new PaymentResponseDto
            {
                Success = true,
                OrderId = cart.Id,
                PaymentMethod = "Visa",
                TransactionId = Guid.NewGuid().ToString(),
                Message = "Payment processed successfully via Visa"
            };
        }

        throw new InvalidOperationException("Visa payment processing failed");
    }

    private async Task<Order> ValidateCustomerAsync(Guid customerId)
    {
        // Add customer validation logic if needed
        // For now, just return - customer validation might be handled elsewhere
        await Task.CompletedTask;
        return new Order();
    }

    private async Task ValidateCustomerCartAsync(Guid customerId)
    {
        var cart = await _unitOfWork.Orders.GetCartByCustomerAsync(customerId);
        if (cart == null || !cart.OrderGames.Any())
        {
            throw new ValidationException("Cart is empty. Cannot process payment for empty cart.");
        }
    }

    private async Task<Order> GetCustomerCartAsync(Guid customerId)
    {
        var cart = await _unitOfWork.Orders.GetCartByCustomerAsync(customerId);
        if (cart == null || !cart.OrderGames.Any())
        {
            throw new ValidationException("Cart is empty. Cannot process payment for empty cart.");
        }
        return cart;
    }

    private async Task CreatePaymentTransactionAsync(Guid orderId, decimal amount, string paymentMethod, Guid customerId)
    {
        var transaction = new PaymentTransaction
        {
            OrderId = orderId,
            CustomerId = customerId,
            Amount = amount,
            PaymentMethod = paymentMethod,
            Status = PaymentStatus.Completed,
            ProcessedAt = DateTime.UtcNow,
            ExternalTransactionId = Guid.NewGuid().ToString()
        };

        await _unitOfWork.PaymentTransactions.AddAsync(transaction);
    }
}