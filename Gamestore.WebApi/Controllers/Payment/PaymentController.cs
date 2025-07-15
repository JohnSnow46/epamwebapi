// W PaymentController - ProcessPayment method
using Gamestore.Entities.Auth;
using Gamestore.Entities.ErrorModels;
using Gamestore.Services.Dto.PaymentDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

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

        // Format response according to README requirements
        return paymentRequest.Method.ToLowerInvariant() switch
        {
            "ibox terminal" => Ok(new
            {
                userId = paymentResult.UserId,
                orderId = paymentResult.OrderId,
                paymentDate = paymentResult.PaymentDate,
                sum = paymentResult.Sum
            }),
            "visa" => Ok(new
            {
                success = true,
                orderId = paymentResult.OrderId,
                transactionId = paymentResult.TransactionId
            }),
            "bank" => File(paymentResult.InvoiceFile, "application/pdf", $"invoice_{paymentResult.OrderId}.pdf"),
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
    catch (KeyNotFoundException ex)
    {
        return NotFound(new ErrorResponseModel
        {
            Message = ex.Message,
            StatusCode = StatusCodes.Status404NotFound
        });
    }
    catch (ValidationException ex)
    {
        return BadRequest(new ErrorResponseModel
        {
            Message = ex.Message,
            StatusCode = StatusCodes.Status400BadRequest
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error processing payment for user {UserEmail}", User.GetUserEmail());
        return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponseModel
        {
            Message = "An error occurred while processing your request.",
            StatusCode = StatusCodes.Status500InternalServerError
        });
    }
}