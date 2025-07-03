using Gamestore.Entities.ErrorModels;
using Gamestore.Services.Dto.FiltersDto;
using Gamestore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Gamestore.Services.Services.Auth;
using Gamestore.WebApi.Extensions;

namespace Gamestore.WebApi.Controllers.Filters;

[ApiController]
[Route("api/games-filter")]
public class GameFilterController(IGameFilterService gameFilterService, ILogger<GameFilterController> logger) : ControllerBase
{
    private readonly IGameFilterService _gameFilterService = gameFilterService;
    private readonly ILogger<GameFilterController> _logger = logger;

    /// <summary>
    /// Epic 9: Everyone can get pagination options
    /// </summary>
    /// <returns>A list of pagination options.</returns>
    [HttpGet("pagination-options")]
    [AllowAnonymous]
    public IActionResult GetPaginationOptions()
    {
        try
        {
            _logger.LogInformation("Getting pagination options");
            var options = _gameFilterService.GetPaginationOptions();
            return Ok(options);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error retrieving pagination options");
        }
    }

    /// <summary>
    /// Epic 9: Everyone can get sorting options
    /// </summary>
    /// <returns>A list of sorting options.</returns>
    [HttpGet("sorting-options")]
    [AllowAnonymous]
    public IActionResult GetSortingOptions()
    {
        try
        {
            _logger.LogInformation("Getting sorting options");
            var options = _gameFilterService.GetSortingOptions();
            return Ok(options);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error retrieving sorting options");
        }
    }

    /// <summary>
    /// Epic 9: Everyone can get publish date filter options
    /// </summary>
    /// <returns>A list of publish date filter options.</returns>
    [HttpGet("publish-date-options")]
    [AllowAnonymous]
    public IActionResult GetPublishDateOptions()
    {
        try
        {
            _logger.LogInformation("Getting publish date filter options");
            var options = _gameFilterService.GetPublishDateFilterOptions();
            return Ok(options);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error retrieving publish date filter options");
        }
    }

    /// <summary>
    /// Epic 9: Everyone can get filtered games. 
    /// Note: Deleted games visibility is handled in the service layer based on user role.
    /// </summary>
    /// <param name="parameters">The filter parameters.</param>
    /// <returns>The filtered games.</returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetFilteredGames([FromQuery] GameFilterParameters parameters)
    {
        try
        {
            var userEmail = User.Identity?.IsAuthenticated == true ? User.GetUserEmail() : "Anonymous";
            var userRole = User.Identity?.IsAuthenticated == true ? User.GetUserRole() : Roles.Guest;

            _logger.LogInformation("Getting filtered games with parameters: {@Parameters} for user: {User} with role: {Role}",
                parameters, userEmail, userRole);

            var result = await _gameFilterService.GetFilteredGamesAsync(parameters);

            // Future enhancement: Filter out deleted games for non-Admin users
            // This would require adding IsDeleted property to Game entity and filtering logic

            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error retrieving filtered games");
        }
    }

    /// <summary>
    /// Epic 9: Authenticated users can increment view count
    /// </summary>
    /// <param name="key">The game key.</param>
    /// <returns>A confirmation message.</returns>
    [HttpPost("{key}/view")]
    [Authorize]
    public async Task<IActionResult> IncrementGameViewCount(string key)
    {
        try
        {
            _logger.LogInformation("Incrementing view count for game with key: {Key} by user: {User}",
                key, User.GetUserEmail());

            await _gameFilterService.IncrementGameViewCountAsync(key);

            return Ok(new
            {
                message = $"View count for game '{key}' incremented successfully",
                user = User.GetUserName(),
                timestamp = DateTime.UtcNow
            });
        }
        catch (KeyNotFoundException ex)
        {
            return ResourceNotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error incrementing view count for game with key: {key}");
        }
    }

    private NotFoundObjectResult ResourceNotFound(string message)
    {
        _logger.LogWarning(message);

        return NotFound(new ErrorResponseModel
        {
            Message = message,
            StatusCode = StatusCodes.Status404NotFound,
        });
    }

    private ObjectResult HandleException(Exception ex, string logMessage)
    {
        _logger.LogError(ex, "{LogMessage}: {ErrorMessage}", logMessage, ex.Message);

        return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponseModel
        {
            Message = "An error occurred.",
            Details = ex.Message,
            StatusCode = StatusCodes.Status500InternalServerError,
        });
    }
}