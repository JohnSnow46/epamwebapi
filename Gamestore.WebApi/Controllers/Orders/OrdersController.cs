using Gamestore.Entities.ErrorModels;
using Gamestore.Services.Interfaces;
using Gamestore.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebApi.Controllers.Orders;
[ApiController]
[Route("api/orders")]
public class OrdersController(IOrderService orderService, ILogger<OrdersController> logger) : ControllerBase
{
    private readonly IOrderService _orderService = orderService;
    private readonly ILogger<OrdersController> _logger = logger;

    /// <summary>
    /// E05 US2 - Get paid and cancelled orders
    /// Epic 9: Authenticated users can view their order history
    /// </summary>
    [HttpGet]
    [Authorize]
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

            _logger.LogInformation("Getting orders for user {UserEmail}", User.GetUserEmail());

            var orders = await _orderService.GetPaidAndCancelledOrdersAsync(customerId.Value);

            // Transform to match Epic 5 response format
            var response = orders.Select(o => new
            {
                id = o.Id,
                customerId = o.CustomerId,
                date = o.Date
            });

            return Ok(response);
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

            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound(new ErrorResponseModel
                {
                    Message = $"Order with ID '{id}' not found",
                    StatusCode = StatusCodes.Status404NotFound
                });
            }

            // Security check: ensure user can only access their own orders
            if (order.CustomerId != customerId.Value)
            {
                return Forbid("You can only access your own orders");
            }

            // Transform to match Epic 5 response format
            var response = new
            {
                id = order.Id,
                customerId = order.CustomerId,
                date = order.Date
            };

            return Ok(response);
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

            // First verify the order exists and belongs to the user
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound(new ErrorResponseModel
                {
                    Message = $"Order with ID '{id}' not found",
                    StatusCode = StatusCodes.Status404NotFound
                });
            }

            if (order.CustomerId != customerId.Value)
            {
                return Forbid("You can only access your own order details");
            }

            var orderDetails = await _orderService.GetOrderDetailsAsync(id);

            // Transform to match Epic 5 response format
            var response = orderDetails.Select(od => new
            {
                productId = od.ProductId,
                price = od.Price,
                quantity = od.Quantity,
                discount = od.Discount
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error retrieving order details for order {id}");
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