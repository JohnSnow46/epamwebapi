using Gamestore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebApi.Controllers;

/// <summary>
/// Controller for Order operations including history from both databases
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderHistoryService _orderHistoryService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderHistoryService orderHistoryService,
        ILogger<OrdersController> logger)
    {
        _orderHistoryService = orderHistoryService;
        _logger = logger;
    }

    /// <summary>
    /// E08 US2 - Get orders history from both databases
    /// URL: GET /api/orders/history?startDate=2023-01-01&endDate=2023-12-31
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetOrdersHistory([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            _logger.LogInformation("GET /api/orders/history called - StartDate: {StartDate}, EndDate: {EndDate}",
                startDate, endDate);

            var orderHistory = await _orderHistoryService.GetOrderHistoryAsync(startDate, endDate);

            _logger.LogInformation("Returning {Count} orders from history", orderHistory.Count());

            return Ok(orderHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetOrdersHistory endpoint");

            return StatusCode(500, new
            {
                message = "An error occurred while fetching order history",
                error = ex.Message
            });
        }
    }
}