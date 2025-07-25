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

            var paymentMethods = await _paymentService.GetAvailablePaymentMethodsAsync(customerId.Value);

            return Ok(paymentMethods);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error retrieving payment methods");
        }
    }

    /// <summary>
    /// E05 US6, US7, US8 - Process payment for order
    /// Single endpoint for all payment methods with different response formats per README
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
            return paymentRequest.Method.ToLowerInvariant() switch
            {
                // US6: Bank payment - return PDF file for download
                "bank" => File(paymentResult.InvoiceFile!, "application/pdf",
                    $"invoice_{paymentResult.OrderId}.pdf"),
                // US7: IBox terminal - specific response format from README
                "ibox terminal" => Ok(new
                {
                    userId = paymentResult.UserId,
                    orderId = paymentResult.OrderId,
                    paymentDate = paymentResult.PaymentDate,
                    sum = paymentResult.Sum
                }),
                // US8: Visa payment - success status code only
                "visa" => Ok(),
                // Fallback for any other payment methods
                _ => Ok(new
                {
                    success = paymentResult.Success,
                    orderId = paymentResult.OrderId,
                    message = paymentResult.Message
                })
            };
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error processing payment");
        }
    }

    /// <summary>
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