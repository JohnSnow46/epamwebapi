﻿using Gamestore.Entities.ErrorModels;
using Gamestore.Services.Interfaces;
using Gamestore.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebApi.Controllers.Orders;

[ApiController]
[Route("api/orders")]
public class OrdersController(IOrderService orderService,
    IOrderHistoryService orderHistoryService,
    ILogger<OrdersController> logger) : ControllerBase
{
    private readonly IOrderService _orderService = orderService;
    private readonly IOrderHistoryService _orderHistoryService = orderHistoryService;
    private readonly ILogger<OrdersController> _logger = logger;

    /// <summary>
    /// E05 US2 - Get paid and cancelled orders
    /// Epic 9: Authenticated users can view their order history
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetOrders()
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

            _logger.LogInformation("Getting orders history for user {UserEmail}", User.GetUserEmail());

            var orders = await _orderHistoryService.GetOrderHistoryAsync();

            return Ok(orders);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error retrieving orders");
        }
    }

    /// <summary>
    /// E05 US2 - Get order by ID
    /// Epic 9: Users can view their own orders
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetOrderById(Guid id)
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

            _logger.LogInformation("Getting order {OrderId} for user {UserEmail}", id, User.GetUserEmail());

            // Service handles authorization internally
            var order = await _orderService.GetOrderByIdAsync(id, customerId.Value);
            if (order == null)
            {
                return NotFound(new ErrorResponseModel
                {
                    Message = $"Order with ID '{id}' not found or access denied",
                    StatusCode = StatusCodes.Status404NotFound
                });
            }

            var response = new
            {
                id = order.Id,
                customerId = order.CustomerId,
                date = order.Date
            };

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error retrieving order {id}");
        }
    }

    /// <summary>
    /// E05 US3 - Get order details
    /// Epic 9: Users can view their order details
    /// </summary>
    [HttpGet("{id}/details")]
    [Authorize]
    public async Task<IActionResult> GetOrderDetails(Guid id)
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

            _logger.LogInformation("Getting order details for order {OrderId} by user {UserEmail}",
                id, User.GetUserEmail());

            // Service handles authorization internally
            var orderDetails = await _orderService.GetOrderDetailsAsync(id, customerId.Value);

            var response = orderDetails.Select(od => new
            {
                productId = od.ProductId,
                price = od.Price,
                quantity = od.Quantity,
                discount = od.Discount
            });

            return Ok(response);
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
        catch (Exception ex)
        {
            return HandleException(ex, $"Error retrieving order details for order {id}");
        }
    }

    /// <summary>
    /// E08 US2 - Get orders history from both databases (Epic 8)
    /// URL: GET /api/orders/history?startDate=2023-01-01&amp;endDate=2023-12-31
    /// UWAGA: UI wysyła parametry "start" i "end", nie "startDate" i "endDate"
    /// </summary>
    [HttpGet("history")]
    [AllowAnonymous]
    public async Task<IActionResult> GetOrdersHistory(
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        [FromQuery] string? start = null,
        [FromQuery] string? end = null)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    {
        try
        {
            _logger.LogInformation("GET /api/orders/history called - Start: {Start}, End: {End}",
                start, end);

            // Konwertuj string parametry na DateTime?
            DateTime? startDate = null;
            DateTime? endDate = null;

            if (!string.IsNullOrEmpty(start))
            {
                if (DateTime.TryParse(start, out var parsedStart))
                {
                    startDate = parsedStart;
                }
                else
                {
                    _logger.LogWarning("Could not parse start date: {Start}", start);
                    return BadRequest(new ErrorResponseModel
                    {
                        Message = $"Invalid start date format: {start}",
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                }
            }

            if (!string.IsNullOrEmpty(end))
            {
                if (DateTime.TryParse(end, out var parsedEnd))
                {
                    endDate = parsedEnd;
                }
                else
                {
                    _logger.LogWarning("Could not parse end date: {End}", end);
                    return BadRequest(new ErrorResponseModel
                    {
                        Message = $"Invalid end date format: {end}",
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                }
            }

            // Walidacja dat
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                return BadRequest(new ErrorResponseModel
                {
                    Message = "Start date cannot be later than end date",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            _logger.LogInformation("Parsed dates - StartDate: {StartDate}, EndDate: {EndDate}",
                startDate, endDate);

            var orderHistory = await _orderHistoryService.GetOrderHistoryAsync(startDate, endDate);

            _logger.LogInformation("Returning {Count} orders from history", orderHistory.Count());

            return Ok(orderHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetOrdersHistory endpoint");

            return StatusCode(500, new ErrorResponseModel
            {
                Message = "An error occurred while fetching order history",
                Details = ex.Message,
                StatusCode = StatusCodes.Status500InternalServerError
            });
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