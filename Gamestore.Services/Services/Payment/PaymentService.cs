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

        await ValidateCustomerAsync(customerId);

        var paymentMethodsFromDb = await _unitOfWork.PaymentMethods.GetActivePaymentMethodsAsync();

        var paymentMethods = paymentMethodsFromDb.Select(pm => new PaymentMethodDto
        {
            Title = pm.Title,
            Description = pm.Description,
            ImageUrl = pm.ImageUrl
        }).ToList();

        return new PaymentMethodsResponseDto { PaymentMethods = paymentMethods };
    }

    public async Task<PaymentResponseDto> ProcessPaymentAsync(PaymentRequestDto paymentRequest, Guid customerId)
    {
        _logger.LogInformation("Processing payment with method {Method} for customer {CustomerId}",
            paymentRequest.Method, customerId);

        var isSupported = await _unitOfWork.PaymentMethods.IsPaymentMethodSupportedAsync(paymentRequest.Method);
        if (!isSupported)
        {
            throw new ValidationException($"Payment method '{paymentRequest.Method}' is not supported or currently unavailable");
        }

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

        var paymentContext = await GetPaymentContextAsync(customerId);

        // Create BankInvoiceDto according to interface
        var bankInvoice = new BankInvoiceDto
        {
            UserId = customerId,
            OrderId = paymentContext.Cart.Id,
            CreationDate = DateTime.UtcNow,
            ValidityDate = DateTime.UtcNow.AddDays(_bankInvoiceValidityDays),
            Sum = paymentContext.OrderTotal
        };

        var invoicePdf = await _pdfGenerator.GenerateBankInvoicePdfAsync(bankInvoice);

        // Bank payments use Checkout status instead of Paid
        await FinalizePaymentAsync(paymentContext, "Bank", OrderStatus.Checkout, createTransaction: false);

        return new PaymentResponseDto
        {
            Success = true,
            OrderId = paymentContext.Cart.Id,
            PaymentMethod = "Bank",
            InvoiceFile = invoicePdf,
            Message = "Bank invoice generated successfully. Please complete payment within 30 days."
        };
    }

    public async Task<PaymentResponseDto> ProcessIBoxPaymentAsync(Guid customerId)
    {
        _logger.LogInformation("Processing IBox payment for customer {CustomerId}", customerId);

        var paymentContext = await GetPaymentContextAsync(customerId);

        var iboxRequest = new BoxMicroserviceRequestDto
        {
            TransactionAmount = paymentContext.OrderTotal,
            AccountNumber = customerId,
            InvoiceNumber = paymentContext.Cart.Id
        };

        var success = await _microserviceClient.ProcessIBoxPaymentAsync(iboxRequest);

        if (success)
        {
            await FinalizePaymentAsync(paymentContext, "IBox Terminal", OrderStatus.Paid, createTransaction: true);

            return new PaymentResponseDto
            {
                Success = true,
                UserId = customerId,
                OrderId = paymentContext.Cart.Id,
                PaymentDate = DateTime.UtcNow,
                Sum = paymentContext.OrderTotal
            };
        }

        throw new InvalidOperationException("IBox payment processing failed");
    }

    public async Task<PaymentResponseDto> ProcessVisaPaymentAsync(Guid customerId, VisaPaymentModelDto visaData)
    {
        _logger.LogInformation("Processing Visa payment for customer {CustomerId}", customerId);

        var paymentContext = await GetPaymentContextAsync(customerId);

        var visaRequest = new VisaMicroserviceRequestDto
        {
            TransactionAmount = paymentContext.OrderTotal,
            CardHolderName = visaData.Holder,
            CardNumber = visaData.CardNumber,
            ExpirationMonth = visaData.MonthExpire,
            ExpirationYear = visaData.YearExpire,
            Cvv = visaData.Cvv2
        };

        var success = await _microserviceClient.ProcessVisaPaymentAsync(visaRequest);

        if (success)
        {
            await FinalizePaymentAsync(paymentContext, "Visa Card", OrderStatus.Paid, createTransaction: true);

            return new PaymentResponseDto
            {
                Success = true,
                OrderId = paymentContext.Cart.Id,
                PaymentMethod = "Visa Card",
                TransactionId = Guid.NewGuid().ToString(),
                Message = "Payment processed successfully via Visa Card"
            };
        }

        throw new InvalidOperationException("Visa payment processing failed");
    }

    /// <summary>
    /// Gets the payment context containing cart and order total for payment processing.
    /// This eliminates code duplication across all payment methods.
    /// </summary>
    private async Task<PaymentContext> GetPaymentContextAsync(Guid customerId)
    {
        var cart = await GetCustomerCartAsync(customerId);
        var orderTotal = await _unitOfWork.OrderGames.GetOrderTotalAsync(cart.Id);

        return new PaymentContext
        {
            Cart = cart,
            OrderTotal = orderTotal,
            CustomerId = customerId
        };
    }

    /// <summary>
    /// Finalizes payment processing by updating order status, creating transaction records, and completing unit of work.
    /// This eliminates code duplication for the common finalization steps across payment methods.
    /// </summary>
    private async Task FinalizePaymentAsync(PaymentContext context, string paymentMethod, OrderStatus orderStatus, bool createTransaction)
    {
        // Update order status
        await _unitOfWork.Orders.UpdateOrderStatusAsync(context.Cart.Id, orderStatus);

        // Create payment transaction record if needed
        if (createTransaction)
        {
            await CreatePaymentTransactionAsync(context.Cart.Id, context.OrderTotal, paymentMethod, context.CustomerId);
        }

        // Complete unit of work
        await _unitOfWork.CompleteAsync();
    }

    private async Task ValidateCustomerAsync(Guid customerId)
    {
        _logger.LogDebug("Validating customer {CustomerId}", customerId);

        // Check if customer exists in the system
        var customer = await _unitOfWork.Users.GetByIdAsync(customerId);
        if (customer == null)
        {
            _logger.LogWarning("Customer {CustomerId} not found", customerId);
            throw new KeyNotFoundException($"Customer with ID {customerId} not found");
        }

        _logger.LogDebug("Customer {CustomerId} validation successful", customerId);
    }

    private async Task ValidateCustomerCartAsync(Guid customerId)
    {
        var cart = await _unitOfWork.Orders.GetCartByCustomerAsync(customerId);
        if (cart == null || cart.OrderGames.Count == 0)
        {
            throw new ValidationException("Cart is empty. Cannot process payment for empty cart.");
        }
    }

    private async Task<Order> GetCustomerCartAsync(Guid customerId)
    {
        var cart = await _unitOfWork.Orders.GetCartByCustomerAsync(customerId);
        return cart == null || cart.OrderGames.Count == 0
            ? throw new ValidationException("Cart is empty. Cannot process payment for empty cart.")
            : cart;
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

    /// <summary>
    /// Context object containing common data needed for payment processing.
    /// Helps eliminate code duplication by centralizing shared payment data.
    /// </summary>
    private sealed class PaymentContext
    {
        public required Order Cart { get; init; }
        public required decimal OrderTotal { get; init; }
        public required Guid CustomerId { get; init; }
    }
}