using Gamestore.Entities.Business;
using Gamestore.Entities.ErrorModels;
using Gamestore.Services.Dto.PlatformsDto;
using Gamestore.Services.Interfaces;
using Gamestore.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebApi.Controllers.Business;

[Route("api/platforms")]
[ApiController]
public class PlatformController(IGameService gameService, IPlatformService platformService, ILogger<PlatformController> logger) : ControllerBase
{
    private readonly IPlatformService _platformService = platformService;
    private readonly IGameService _gameService = gameService;
    private readonly ILogger<PlatformController> _logger = logger;

    /// <summary>
    /// Epic 9: Admin and Manager can create platforms.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "CanManageBusinessEntities")]
    public async Task<IActionResult> CreatePlatform([FromBody] PlatformMetadataCreateRequestDto platformRequest)
    {
        try
        {
            if (platformRequest?.Platform == null)
            {
                return BadRequest(new ErrorResponseModel
                {
                    Message = "Platform data is required.",
                    StatusCode = StatusCodes.Status400BadRequest,
                });
            }

            _logger.LogInformation(
                "Creating platform with Type: {PlatformType} by user: {User}",
                platformRequest.Platform.Type,
                User.GetUserEmail());

            var updatedPlatform = await _platformService.CreatePlatform(platformRequest);

            if (updatedPlatform == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponseModel
                {
                    Message = "Failed to create the platform.",
                    StatusCode = StatusCodes.Status500InternalServerError,
                });
            }

            _logger.LogInformation("Successfully created platform with ID: {PlatformId}", updatedPlatform.Id);
            return Ok(updatedPlatform);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error creating platform");
        }
    }

    /// <summary>
    /// Epic 9: Admin and Manager can update platforms.
    /// </summary>
    [HttpPut]
    [Authorize(Policy = "CanManageBusinessEntities")]
    public async Task<IActionResult> UpdatePlatform([FromBody] PlatformMetadataUpdateRequestDto platformUpdateDto)
    {
        try
        {
            if (platformUpdateDto?.Platform == null)
            {
                return BadRequest(new ErrorResponseModel
                {
                    Message = "Platform data is required.",
                    StatusCode = StatusCodes.Status400BadRequest,
                });
            }

            if (platformUpdateDto.Platform.Id == Guid.Empty)
            {
                return BadRequest(new ErrorResponseModel
                {
                    Message = "Invalid platform data or missing ID.",
                    StatusCode = StatusCodes.Status400BadRequest,
                });
            }

            _logger.LogInformation(
                "Received platform update request for ID: {PlatformId} from user: {User}",
                platformUpdateDto.Platform.Id,
                User.GetUserEmail());

            var updatedPlatform = await _platformService.UpdatePlatform(platformUpdateDto.Platform.Id, platformUpdateDto.Platform);

            if (updatedPlatform == null)
            {
                return ResourceNotFound($"Platform with ID '{platformUpdateDto.Platform.Id}' not found.");
            }

            _logger.LogInformation(
                "Successfully updated platform with ID: {PlatformId} by user: {User}",
                updatedPlatform.Platform.Id,
                User.GetUserEmail());
            return Ok(updatedPlatform);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error updating platform");
        }
    }

    /// <summary>
    /// Epic 9: Admin and Manager can delete platforms.
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
    /// Epic 9: Everyone can view platforms.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllPlatforms()
    {
        try
        {
            _logger.LogInformation("Getting all platforms");
            var platforms = await _platformService.GetAllPlatformsAsync();

            _logger.LogInformation("Successfully retrieved {Count} platforms", platforms.Count());
            return Ok(platforms ?? Enumerable.Empty<Platform>());
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error retrieving all platforms");
        }
    }

    /// <summary>
    /// Epic 9: Everyone can view platform by ID.
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
    /// Epic 9: Everyone can view platforms by game key.
    /// </summary>
    [HttpGet("game/{key}")]
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
    /// Epic 9: Everyone can view games by platform.
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

    /// <summary>
    /// US14 - Delete platform endpoint
    /// Epic 10: Admin and Manager can delete platforms.
    /// </summary>
    [HttpDelete("platforms/{id}")]
    [Authorize(Policy = "CanManageBusinessEntities")]
    public async Task<IActionResult> DeletePlatform(Guid id)
    {
        try
        {
            _logger.LogInformation(
                "Deleting platform with ID: {PlatformId} by user: {User}",
                id,
                User.GetUserEmail());

            var platform = await _platformService.DeletePlatformById(id);

            if (platform == null)
            {
                return NotFound(new ErrorResponseModel
                {
                    Message = $"Platform with ID '{id}' not found",
                    StatusCode = StatusCodes.Status404NotFound,
                });
            }

            _logger.LogInformation("Successfully deleted platform with ID: {PlatformId}", id);
            return Ok(platform);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting platform with ID: {PlatformId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponseModel
            {
                Message = "An error occurred while deleting the platform",
                StatusCode = StatusCodes.Status500InternalServerError,
            });
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