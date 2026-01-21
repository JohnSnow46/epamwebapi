using Gamestore.Entities.ErrorModels;
using Gamestore.Services.Interfaces;
using Gamestore.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebApi.Controllers.Business;

[Route("api")]
[ApiController]
public class ShoppingController(
    IShoppingService shoppingService,
    ILogger<ShoppingController> logger) : ControllerBase
{
    private readonly IShoppingService _shoppingService = shoppingService;
    private readonly ILogger<ShoppingController> _logger = logger;

    [HttpGet("orders/cart")]
    [Authorize]
    public async Task<IActionResult> GetBasket()
    {
        try
        {
            var customerId = User.GetUserEmail() ?? string.Empty;
            _logger.LogInformation("Getting basket for customer: {CustomerId}", customerId);

            var basket = await _shoppingService.GetBasketAsync(customerId);
            return Ok(basket);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error retrieving basket");
        }
    }

    [HttpPost("games/{key}/buy")]
    [Authorize]
    public async Task<IActionResult> BuyGame(string key)
    {
        try
        {
            var customerId = User.GetUserEmail() ?? string.Empty;
            _logger.LogInformation("Customer {CustomerId} buying game {GameKey}", customerId, key);

            var result = await _shoppingService.BuyGameAsync(key, customerId);

            return !result
                ? BadRequest(new ErrorResponseModel { Message = "Failed to add game to basket" })
                : Ok(new { message = "Game added to basket" });
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error buying game {key}");
        }
    }

    [HttpDelete("orders/cart/{key}")]
    [Authorize]
    public async Task<IActionResult> CancelGameBuy(string key)
    {
        try
        {
            var customerId = User.GetUserEmail() ?? string.Empty;
            _logger.LogInformation("Customer {CustomerId} removing game {GameKey} from basket", customerId, key);

            var result = await _shoppingService.CancelGameBuyAsync(key, customerId);

            return !result
                ? BadRequest(new ErrorResponseModel { Message = "Game not found in basket" })
                : Ok(new { message = "Game removed from basket" });
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error removing game {key} from basket");
        }
    }

    private ObjectResult HandleException(Exception ex, string message)
    {
        _logger.LogError(ex, "{Message}", message);
        return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponseModel
        {
            Message = message,
            StatusCode = StatusCodes.Status500InternalServerError,
        });
    }
}