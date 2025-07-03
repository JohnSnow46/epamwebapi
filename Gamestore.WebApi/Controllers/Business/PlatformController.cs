using System.Text.Json;
using Gamestore.Entities.ErrorModels;
using Gamestore.Services.Dto.PlatformsDto;
using Gamestore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Gamestore.WebApi.Extensions;
using Gamestore.Entities.Business;

namespace Gamestore.WebApi.Controllers.Business;

[Route("api/platforms")]
[ApiController]
public class PlatformController(IGameService gameService, IPlatformService platformService, ILogger<PlatformController> logger) : ControllerBase
{
    private readonly IPlatformService _platformService = platformService;
    private readonly IGameService _gameService = gameService;
    private readonly ILogger<PlatformController> _logger = logger;

    /// <summary>
    /// Epic 9: Admin and Manager can create platforms
    /// </summary>
    [HttpPost("add-platform")]
    [Authorize(Policy = "CanManageBusinessEntities")]
    public async Task<IActionResult> CreateOrUpdatePlatform([FromBody] PlatformMetadataCreateRequestDto platformRequest)
    {
        try
        {
            _logger.LogInformation("Creating or updating platform with Type: {PlatformType} by user: {User}",
                platformRequest.Platform.Type, User.GetUserEmail());

            var updatedPlatform = await _platformService.CreatePlatform(platformRequest);

            if (updatedPlatform == null)
            {
                return ResourceNotFound($"Platform with ID '{platformRequest.Platform.Id}' not found.");
            }

            _logger.LogInformation("Successfully processed platform with ID: {PlatformId}", updatedPlatform.Id);
            return Ok(updatedPlatform);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error creating or updating platform");
        }
    }

    /// <summary>
    /// Epic 9: Admin and Manager can update platforms
    /// </summary>
    [HttpPut("update-platform")]
    [Authorize(Policy = "CanManageBusinessEntities")]
    public async Task<IActionResult> UpdatePlatform([FromBody] JsonElement requestData)
    {
        try
        {
            _logger.LogInformation("Received platform update request from user: {User}", User.GetUserEmail());

            if (!requestData.TryGetProperty("platform", out var platformElement))
            {
                return BadRequest(new ErrorResponseModel
                {
                    Message = "Invalid request format. Expected 'platform' property.",
                    StatusCode = StatusCodes.Status400BadRequest,
                });
            }

            var platformUpdateDto = platformElement.Deserialize<PlatformMetadataUpdateRequestDto>();
            if (platformUpdateDto == null || platformUpdateDto.Id == Guid.Empty)
            {
                return BadRequest(new ErrorResponseModel
                {
                    Message = "Invalid platform data or missing ID.",
                    StatusCode = StatusCodes.Status400BadRequest,
                });
            }

            var id = platformUpdateDto.Id;
            _logger.LogInformation("Updating platform with ID: {PlatformId}, Type: {PlatformType}", id, platformUpdateDto.Type);

            var updatedPlatform = await _platformService.UpdatePlatform(id, platformUpdateDto);

            _logger.LogInformation("Successfully updated platform with ID: {PlatformId}", updatedPlatform.Id);
            return Ok(new { platform = updatedPlatform });
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error updating platform");
        }
    }

    /// <summary>
    /// Epic 9: Admin and Manager can delete platforms
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "CanManageBusinessEntities")]
    public async Task<IActionResult> DeletePlatformById(Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting platform with ID: {PlatformId} by user: {User}", id, User.GetUserEmail());
            var deletedPlatform = await _platformService.DeletePlatformById(id);

            if (deletedPlatform == null)
            {
                return ResourceNotFound($"Platform with ID '{id}' not found.");
            }

            _logger.LogInformation("Successfully deleted platform with ID: {PlatformId}", id);
            return Ok(deletedPlatform);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error deleting platform with ID: {id}");
        }
    }

    /// <summary>
    /// Epic 9: Everyone can view platforms
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Platform>>> GetAllPlatforms()
    {
        try
        {
            _logger.LogInformation("Getting all platforms");
            var platforms = await _platformService.GetAllPlatformsAsync();

            _logger.LogInformation("Successfully retrieved {Count} platforms", platforms.Count());
            return Ok(platforms);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error retrieving all platforms");
        }
    }

    /// <summary>
    /// Epic 9: Everyone can view platform by ID
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPlatformById(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting platform by ID: {PlatformId}", id);
            var platform = await _platformService.GetPlatformById(id);

            if (platform == null)
            {
                return ResourceNotFound($"Platform with ID '{id}' not found.");
            }

            _logger.LogInformation("Successfully retrieved platform with ID: {PlatformId}", id);
            return Ok(platform);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error getting platform by ID: {id}");
        }
    }

    /// <summary>
    /// Epic 9: Everyone can view platforms by game key
    /// </summary>
    [HttpGet("{key}/platforms")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPlatformsByGameKey(string key)
    {
        try
        {
            _logger.LogInformation("Getting platforms for game with key: {GameKey}", key);
            var platforms = await _platformService.GetPlatformsByGameKeyAsync(key);

            if (!platforms.Any())
            {
                return ResourceNotFound($"No platforms found for game '{key}'.");
            }

            _logger.LogInformation("Successfully retrieved {Count} platforms for game with key: {GameKey}", platforms.Count(), key);
            return Ok(platforms);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error retrieving platforms for game with key: {key}");
        }
    }

    /// <summary>
    /// Epic 9: Everyone can view games by platform
    /// </summary>
    [HttpGet("platforms/{id}/games")]
    [AllowAnonymous]
    public async Task<IActionResult> GetGamesByPlatformId(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting games by platform ID: {PlatformId}", id);
            var platform = await _gameService.GetGamesByPlatformAsync(id);

            if (platform == null)
            {
                return ResourceNotFound($"Platform with ID '{id}' not found.");
            }

            _logger.LogInformation("Successfully retrieved games for platform ID: {PlatformId}", id);
            return Ok(platform);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error retrieving games for platform ID: {id}");
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