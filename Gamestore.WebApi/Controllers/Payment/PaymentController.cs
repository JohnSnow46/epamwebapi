using Gamestore.Entities.ErrorModels;
using Gamestore.Services.Dto.PaymentDto;
using Gamestore.Services.Interfaces;
using Gamestore.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebApi.Controllers.Payment;

[ApiController]
[Route("api/orders")]
public class PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger) : ControllerBase
{
    private readonly IPaymentService _paymentService = paymentService;
    private readonly ILogger<PaymentController> _logger = logger;

    /// <summary>
    /// E05 US5 - Get available payment methods
    /// Epic 9: Authenticated users can view payment options
    /// </summary>
    [HttpGet("payment-methods")]
    [Authorize(Policy = "CanBuyGames")]
    public async Task<IActionResult> GetPaymentMethods()
    {
        try
        {
            var customerId = User.GetUserId();
            if (!customerId.HasValue)
            {
                return BadRequest(new ErrorResponseModel
                {
                    Message = "Unable to identify customer",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            _logger.LogInformation("Getting payment methods for user {UserEmail}", User.GetUserEmail());

            // ✅ POPRAWNE: przekazuje customerId do service
            var paymentMethods = await _paymentService.GetAvailablePaymentMethodsAsync(customerId.Value);

            return Ok(paymentMethods);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error retrieving payment methods");
        }
    }

    /// <summary>
    /// E05 US6 - Process payment for order
    /// Epic 9: Authenticated users can complete purchases
    /// </summary>
    [HttpPost("payment")]
    [Authorize(Policy = "CanBuyGames")]
    public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequestDto paymentRequest)
    {
        try
        {
            var customerId = User.GetUserId();
            if (!customerId.HasValue)
            {
                return BadRequest(new ErrorResponseModel
                {
                    Message = "Unable to identify customer",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            _logger.LogInformation("Processing payment for user {UserEmail} with method {PaymentMethod}",
                User.GetUserEmail(), paymentRequest.Method);

            var paymentResult = await _paymentService.ProcessPaymentAsync(paymentRequest, customerId.Value);

            return Ok(new
            {
                success = true,
                orderId = paymentResult.OrderId,
                transactionId = paymentResult.TransactionId,
                message = "Payment processed successfully",
                processedBy = User.GetUserName(),
                processedAt = DateTime.UtcNow
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ErrorResponseModel
            {
                Message = ex.Message,
                StatusCode = StatusCodes.Status404NotFound
            });
        }
        catch (System.ComponentModel.DataAnnotations.ValidationException ex)
        {
            return BadRequest(new ErrorResponseModel
            {
                Message = ex.Message,
                StatusCode = StatusCodes.Status400BadRequest
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponseModel
            {
                Message = ex.Message,
                StatusCode = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error processing payment");
        }
    }

    /// <summary>
    /// E05 US7 - Get payment history for user
    /// Epic 9: Authenticated users can view their payment history
    /// </summary>
    [HttpGet("payment-history")]
    [Authorize]
    public async Task<IActionResult> GetPaymentHistory()
    {
        try
        {
            var customerId = User.GetUserId();
            if (!customerId.HasValue)
            {
                return BadRequest(new ErrorResponseModel
                {
                    Message = "Unable to identify customer",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            _logger.LogInformation("Getting payment history for user {UserEmail}", User.GetUserEmail());

            // ✅ POPRAWNE: service handles authorization internally
            var paymentHistory = await _paymentService.GetPaymentHistoryAsync(customerId.Value);

            return Ok(paymentHistory);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error retrieving payment history");
        }
    }

    private ObjectResult HandleException(Exception ex, string logMessage)
    {
        _logger.LogError(ex, "{LogMessage}: {ErrorMessage}", logMessage, ex.Message);

        return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponseModel
        {
            Message = "An error occurred while processing your request.",
            Details = ex.Message,
            StatusCode = StatusCodes.Status500InternalServerError
        });
    }
}