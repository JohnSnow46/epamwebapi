using Gamestore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebApi.Controllers;

/// <summary>
/// Controller for Shipper operations
/// Implements Epic 8 US1 - Get Shippers endpoint
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ShippersController(
    IShipperService shipperService,
    ILogger<ShippersController> logger) : ControllerBase
{
    private readonly IShipperService _shipperService = shipperService;
    private readonly ILogger<ShippersController> _logger = logger;

    /// <summary>
    /// E08 US1 - Get Shippers endpoint
    /// URL: GET /api/shippers
    /// Response: Dynamic content structure from MongoDB
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetShippers()
    {
        try
        {
            _logger.LogInformation("GET /api/shippers called");

            var shippers = await _shipperService.GetAllShippersAsync();

            _logger.LogInformation("Returning {Count} shippers", shippers.Count());

            return Ok(shippers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetShippers endpoint");

            return StatusCode(500, new
            {
                message = "An error occurred while fetching shippers",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Additional endpoint for testing specific shipper
    /// GET /api/shippers/{id}
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetShipper(int id)
    {
        try
        {
            _logger.LogInformation("GET /api/shippers/{Id} called", id);

            var shipper = await _shipperService.GetShipperByIdAsync(id);

            return shipper == null ? NotFound(new { message = $"Shipper with ID {id} not found" }) : Ok(shipper);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetShipper endpoint for ID {Id}", id);

            return StatusCode(500, new
            {
                message = "An error occurred while fetching shipper",
                error = ex.Message
            });
        }
    }
}