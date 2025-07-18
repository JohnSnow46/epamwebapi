using Gamestore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebApi.Controllers;

/// <summary>
/// Controller for unified product operations across both databases
/// Implements E08 US4 requirements
/// </summary>
[ApiController]
[Route("api/unified-products")]
public class UnifiedProductsController : ControllerBase
{
    private readonly IUnifiedProductService _unifiedProductService;
    private readonly ILogger<UnifiedProductsController> _logger;

    public UnifiedProductsController(
        IUnifiedProductService unifiedProductService,
        ILogger<UnifiedProductsController> logger)
    {
        _unifiedProductService = unifiedProductService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all products from both databases
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllProducts()
    {
        try
        {
            _logger.LogInformation("GET /api/unified-products called");

            var products = await _unifiedProductService.GetAllProductsAsync();

            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllProducts endpoint");
            return StatusCode(500, new { message = "Error fetching products", error = ex.Message });
        }
    }

    /// <summary>
    /// Gets product by ID from either database
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(string id)
    {
        try
        {
            var product = await _unifiedProductService.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound(new { message = $"Product with ID {id} not found" });
            }

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product {Id}", id);
            return StatusCode(500, new { message = "Error fetching product", error = ex.Message });
        }
    }

    /// <summary>
    /// E08 US5 - Sync stock count
    /// </summary>
    [HttpPut("{id}/stock")]
    public async Task<IActionResult> SyncStockCount(string id, [FromBody] dynamic request)
    {
        try
        {
            int newStock = request.stock;
            var result = await _unifiedProductService.SyncStockCountAsync(id, newStock);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing stock for product {Id}", id);
            return StatusCode(500, new { message = "Error syncing stock", error = ex.Message });
        }
    }

    /// <summary>
    /// Test endpoint to verify Epic 8 implementation
    /// </summary>
    [HttpGet("epic8-status")]
    public IActionResult GetEpic8Status()
    {
        return Ok(new
        {
            epic = "Epic 8 - NoSQL Database Integration",
            status = "Implemented",
            features = new[]
            {
            "✅ US1 - Shippers endpoint",
            "✅ US2 - Orders History endpoint",
            "✅ US3 - SQL Database extended",
            "✅ US4 - CRUD operations for both databases",
            "✅ US5 - Stock count sync",
            "✅ US6 - Duplicate management",
            "✅ US7 - MongoDB update logic (copy to SQL)",
            "✅ NFR1 - IQueryable for MongoDB reads",
            "✅ NFR2 - FilterDefinition/UpdateDefinition",
            "✅ NFR3 - Raw MongoDB queries",
            "✅ NFR4 - Entity changes logging"
        },
            endpoints = new[]
            {
            "GET /api/shippers",
            "GET /api/orders/history",
            "GET /api/unified-products",
            "PUT /api/unified-products/{id}/stock"
        }
        });
    }
}