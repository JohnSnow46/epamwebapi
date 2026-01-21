using Gamestore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebApi.Controllers.Business;

[Route("api")]
[ApiController]
public class OrderController(
    IOrderService orderService,
    ILogger<OrderController> logger) : ControllerBase
{
    private readonly IOrderService _orderService = orderService;
    private readonly ILogger<OrderController> _logger = logger;

    /// <summary>
    /// Get all orders - Admin/Manager only.
    /// </summary>
    [HttpGet("orders")]
    [Authorize(Policy = "CanManageOrders")]
    public async Task<IActionResult> GetAllOrders()
    {
        try
        {
            _logger.LogInformation("Getting all orders");
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders");
            return StatusCode(500, "Error retrieving orders");
        }
    }

    /// <summary>
    /// Get order by ID.
    /// </summary>
    [HttpGet("orders/{id}")]
    [Authorize]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting order: {OrderId}", id);
            var order = await _orderService.GetOrderAsync(id);

            return order == null ? NotFound("Order not found") : Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order");
            return StatusCode(500, "Error retrieving order");
        }
    }

    /// <summary>
    /// Ship order.
    /// </summary>
    [HttpPost("orders/{id}/ship")]
    [Authorize(Policy = "CanManageOrders")]
    public async Task<IActionResult> ShipOrder(Guid id)
    {
        try
        {
            _logger.LogInformation("Shipping order: {OrderId}", id);
            var result = await _orderService.ShipOrderAsync(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error shipping order");
            return StatusCode(500, "Error shipping order");
        }
    }

    /// <summary>
    /// Get order details.
    /// </summary>
    [HttpGet("orders/{id}/details")]
    [Authorize]
    public async Task<IActionResult> GetOrderDetails(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting order details for order: {OrderId}", id);
            var details = await _orderService.GetOrderDetailsAsync(id);
            return Ok(details);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order details");
            return StatusCode(500, "Error retrieving order details");
        }
    }
}