using Gamestore.Entities.ErrorModels;
using Gamestore.Services.Dto.CartDto;
using Gamestore.Services.Interfaces;
using Gamestore.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebApi.Controllers.Orders;

[ApiController]
[Route("api")]
public class CartController(ICartService cartService, ILogger<CartController> logger) : ControllerBase
{
    private readonly ICartService _cartService = cartService;
    private readonly ILogger<CartController> _logger = logger;

    /// <summary>
    /// E05 US1 - Add game to cart
    /// Epic 9: Authenticated users (except Guest) can buy games
    /// </summary>
    [HttpPost("games/{key}/buy")]
    [Authorize(Policy = "CanBuyGames")]
    public async Task<IActionResult> AddGameToCart(string key, [FromBody] AddToCartRequestDto request = null)
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

            var quantity = request?.Quantity ?? 1;

            _logger.LogInformation("Adding game {GameKey} to cart for user {UserEmail} with quantity {Quantity}",
                key, User.GetUserEmail(), quantity);

            await _cartService.AddGameToCartAsync(key, customerId.Value, quantity);

            return Ok(new
            {
                success = true,
                message = $"Game '{key}' added to cart successfully",
                quantity,
                addedBy = User.GetUserName(),
                addedAt = DateTime.UtcNow
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ErrorResponseModel
            {
                Message = ex.Message,
                StatusCode = StatusCodes.Status404NotFound
            });
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
            return HandleException(ex, $"Error adding game {key} to cart");
        }
    }

    /// <summary>
    /// E05 US1 - Remove game from cart
    /// Epic 9: Authenticated users can manage their cart
    /// </summary>
    [HttpDelete("orders/cart/{key}")]
    [Authorize(Policy = "CanBuyGames")]
    public async Task<IActionResult> RemoveGameFromCart(string key)
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

            _logger.LogInformation("Removing game {GameKey} from cart for user {UserEmail}",
                key, User.GetUserEmail());

            await _cartService.RemoveGameFromCartAsync(key, customerId.Value);

            return Ok(new
            {
                success = true,
                message = $"Game '{key}' removed from cart successfully",
                removedBy = User.GetUserName(),
                removedAt = DateTime.UtcNow
            });
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
            return HandleException(ex, $"Error removing game {key} from cart");
        }
    }

    /// <summary>
    /// E05 US4 - Get cart contents
    /// Epic 9: Authenticated users can view their cart
    /// </summary>
    [HttpGet("orders/cart")]
    [Authorize(Policy = "CanBuyGames")]
    public async Task<IActionResult> GetCart()
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

            _logger.LogInformation("Getting cart for user {UserEmail}", User.GetUserEmail());

            var cartItems = await _cartService.GetCartAsync(customerId.Value);

            return Ok(cartItems);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error retrieving cart");
        }
    }

    /// <summary>
    /// E05 US5 - Update cart item quantity
    /// Epic 9: Authenticated users can modify their cart
    /// </summary>
    [HttpPut("orders/cart/{key}/quantity")]
    [Authorize(Policy = "CanBuyGames")]
    public async Task<IActionResult> UpdateCartItemQuantity(string key, [FromBody] UpdateCartQuantityRequestDto request)
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

            _logger.LogInformation("Updating cart item quantity for game {GameKey} to {Quantity} for user {UserEmail}",
                key, request.Quantity, User.GetUserEmail());

            await _cartService.UpdateCartItemQuantityAsync(key, customerId.Value, request.Quantity);

            return Ok(new
            {
                success = true,
                message = $"Cart item '{key}' quantity updated successfully",
                newQuantity = request.Quantity,
                updatedBy = User.GetUserName(),
                updatedAt = DateTime.UtcNow
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ErrorResponseModel
            {
                Message = ex.Message,
                StatusCode = StatusCodes.Status404NotFound
            });
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
            return HandleException(ex, $"Error updating cart item quantity for game {key}");
        }
    }

    /// <summary>
    /// E05 US6 - Clear entire cart
    /// Epic 9: Authenticated users can clear their cart
    /// </summary>
    [HttpDelete("orders/cart")]
    [Authorize(Policy = "CanBuyGames")]
    public async Task<IActionResult> ClearCart()
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

            _logger.LogInformation("Clearing cart for user {UserEmail}", User.GetUserEmail());

            await _cartService.ClearCartAsync(customerId.Value);

            return Ok(new
            {
                success = true,
                message = "Cart cleared successfully",
                clearedBy = User.GetUserName(),
                clearedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error clearing cart");
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