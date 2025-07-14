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
    /// E05 US5 - Get payment methods
    /// Epic 9: Everyone can view available payment methods
    /// </summary>
    [HttpGet("payment-methods")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPaymentMethods()
    {
        try
        {
            _logger.LogInformation("Getting available payment methods");

            var paymentMethods = await _paymentService.GetPaymentMethodsAsync();
            return Ok(paymentMethods);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error retrieving payment methods");
        }
    }

    /// <summary>
    /// E05 US6, US7, US8 - Process payment (Bank, IBox, Visa)
    /// Epic 9: Authenticated users can process payments
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

            _logger.LogInformation("Processing {PaymentMethod} payment for user {UserEmail}",
                paymentRequest.Method, User.GetUserEmail());

            if (paymentRequest.Method.Equals("Visa", StringComparison.OrdinalIgnoreCase)
                && paymentRequest.Model == null)
            {
                return BadRequest(new ErrorResponseModel
                {
                    Message = "Visa payment requires card details",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var result = await _paymentService.ProcessPaymentAsync(customerId.Value, paymentRequest);

            if (result.Success)
            {
                _logger.LogInformation("Payment processed successfully for user {UserEmail} using {PaymentMethod}",
                    User.GetUserEmail(), paymentRequest.Method);

                return paymentRequest.Method.ToLowerInvariant() switch
                {
                    "bank" => HandleBankPaymentResponse(result),
                    "ibox terminal" => Ok(result.Data),
                    "visa" => Ok(),
                    _ => Ok(result)
                };
            }
            else
            {
                _logger.LogWarning("Payment failed for user {UserEmail} using {PaymentMethod}: {Message}",
                    User.GetUserEmail(), paymentRequest.Method, result.Message);

                return BadRequest(new ErrorResponseModel
                {
                    Message = result.Message,
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }
        }
        catch (System.ComponentModel.DataAnnotations.ValidationException ex)
        {
            return BadRequest(new ErrorResponseModel
            {
                Message = ex.Message,
                StatusCode = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error processing {paymentRequest.Method} payment");
        }
    }

    private IActionResult HandleBankPaymentResponse(PaymentResponseDto result)
    {
        _logger.LogInformation("Handling bank payment response. Success: {Success}, HasData: {HasData}",
            result.Success, result.Data != null);

        if (result.Data is not null)
        {
            try
            {
                _logger.LogInformation("Data type: {Type}", result.Data.GetType().Name);

                var dataDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(
                    System.Text.Json.JsonSerializer.Serialize(result.Data));

#pragma warning disable CA1854
                if (dataDict != null && dataDict.ContainsKey("InvoiceFile") && dataDict.ContainsKey("FileName"))
                {
                    var invoiceFileBase64 = dataDict["InvoiceFile"].ToString();

                    var invoiceBytes = Convert.FromBase64String(invoiceFileBase64);
                    return File(invoiceBytes, "text/plain", "invoice.txt");
                }
                else
                {
                    _logger.LogWarning("Bank payment data missing InvoiceFile or FileName properties");
                    return Ok(result);
                }
#pragma warning restore CA1854
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing bank payment response");
                return Ok(result);
            }
        }

        _logger.LogWarning("Bank payment response has no data");
        return Ok(result);
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
