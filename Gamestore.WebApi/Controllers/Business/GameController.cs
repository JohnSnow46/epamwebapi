// WebApi/Controllers/GameController.cs - Enhanced Authorization
using System.Text.Json;
using Gamestore.Entities.ErrorModels;
using Gamestore.Services.Dto.GamesDto;
using Gamestore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Gamestore.WebApi.Extensions;

namespace Gamestore.WebApi.Controllers.Business;

[ApiController]
[Route("api")]
public class GameController(IGameService gameService, IPublisherService publisherService, ILogger<GameController> logger) : ControllerBase
{
    private readonly IGameService _gameService = gameService;
    private readonly IPublisherService _publisherService = publisherService;
    private readonly ILogger<GameController> _logger = logger;

    /// <summary>
    /// Epic 9: Admin and Manager can create games
    /// </summary>
    [HttpPost("games/add-game")]
    [Authorize(Policy = "CanManageGames")]
    public async Task<IActionResult> CreateGame([FromBody] GameMetadataCreateRequestDto gameRequest)
    {
        try
        {
            _logger.LogInformation("Creating game with Name: {GameName} by user: {User} (Role: {Role})",
                gameRequest.Game.Name, User.GetUserEmail(), User.GetUserRole());

            var game = await _gameService.AddGameAsync(gameRequest);

            return game == null ? InternalServerError("Failed to create the game.") : (IActionResult)Ok(game);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error creating game");
        }
    }

    /// <summary>
    /// Epic 9: Admin and Manager can update games. Admin can edit deleted games.
    /// </summary>
    [HttpPut("games/update-game")]
    [Authorize(Policy = "CanManageGames")]
    public async Task<IActionResult> UpdateGame([FromBody] JsonElement requestData)
    {
        try
        {
            _logger.LogInformation("Received game update request from user: {User} (Role: {Role})",
                User.GetUserEmail(), User.GetUserRole());

            if (!requestData.TryGetProperty("game", out _))
            {
                return BadRequest(new ErrorResponseModel
                {
                    Message = "Invalid request format. Expected 'game' property.",
                    StatusCode = StatusCodes.Status400BadRequest,
                });
            }

            var gameUpdateDto = requestData.Deserialize<GameMetadataUpdateRequestDto>();
            if (gameUpdateDto == null || string.IsNullOrEmpty(gameUpdateDto.Game.Key))
            {
                return BadRequest(new ErrorResponseModel
                {
                    Message = "Invalid game data or missing Key.",
                    StatusCode = StatusCodes.Status400BadRequest,
                });
            }

            var key = gameUpdateDto.Game.Key;
            _logger.LogInformation("Updating game with Key: {GameKey} by user: {User}", key, User.GetUserEmail());

            // Check if game exists first
            var existingGame = await _gameService.GetGameByKey(key);
            if (existingGame == null)
            {
                return ResourceNotFound($"Game with key: '{key}' not found.");
            }

            if (User.IsAdmin())
            {
                _logger.LogInformation("Admin {User} updating game - can edit deleted games", User.GetUserEmail());
            }

            var updatedGame = await _gameService.UpdateGameAsync(key, gameUpdateDto);

            _logger.LogInformation("Successfully updated game with Key: {GameKey} by user: {User}",
                updatedGame.Key, User.GetUserEmail());
            return Ok(new { game = updatedGame });
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error updating game");
        }
    }

    /// <summary>
    /// Epic 9: Everyone can view games. Guests have read-only access.
    /// </summary>
    [HttpGet("games/{key}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetGameByKey(string key)
    {
        try
        {
            var userInfo = User.Identity?.IsAuthenticated == true
                ? $"{User.GetUserEmail()} (Role: {User.GetUserRole()})"
                : "Anonymous Guest";

            _logger.LogInformation("Getting game by key: {Key} for user: {UserInfo}", key, userInfo);

            var game = await _gameService.GetGameByKey(key);

            return game == null ? ResourceNotFound($"Game with key: '{key}' not found.") : Ok(game);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error getting game by key: {key}");
        }
    }

    /// <summary>
    /// Epic 9: Everyone can view games
    /// </summary>
    [HttpGet("games/find/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetGameById(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting game by id: {Id}", id);
            var game = await _gameService.GetGameById(id);

            return game == null ? ResourceNotFound($"Game with id: '{id}' not found.") : Ok(game);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error getting game by id: {id}");
        }
    }

    /// <summary>
    /// Epic 9: Admin and Manager can delete games
    /// </summary>
    [HttpDelete("games/{key}")]
    [Authorize(Policy = "CanManageGames")]
    public async Task<IActionResult> DeleteGameByKey(string key)
    {
        try
        {
            _logger.LogInformation("Deleting game by key: {Key} by user: {User} (Role: {Role})",
                key, User.GetUserEmail(), User.GetUserRole());

            if (string.IsNullOrWhiteSpace(key) || key == "undefined")
            {
                return BadRequest(new ErrorResponseModel
                {
                    Message = "Invalid game key provided",
                    Details = "Game key cannot be empty or 'undefined'",
                    StatusCode = StatusCodes.Status400BadRequest,
                });
            }

            var game = await _gameService.DeleteGameAsync(key);
            _logger.LogInformation("Successfully deleted game with key: {Key} by user: {User}",
                key, User.GetUserEmail());
            return Ok(game);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error deleting game by key: {key}");
        }
    }

    /// <summary>
    /// Epic 9: Everyone can view games list
    /// </summary>
    [HttpGet("games/")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllGames()
    {
        try
        {
            _logger.LogInformation("Getting all games for user: {User}",
                User.Identity?.IsAuthenticated == true ? User.GetUserEmail() : "Anonymous");

            var games = await _gameService.GetAllGames();

            if (games == null || !games.Any())
            {
                return ResourceNotFound("No games found.");
            }

            _logger.LogInformation("Successfully retrieved {Count} games", games.Count());
            return Ok(games);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error retrieving all games");
        }
    }

    /// <summary>
    /// Epic 9: Authenticated users can download game files
    /// </summary>
    [HttpGet("games/download-game-file/{key}")]
    [Authorize]
    public async Task<IActionResult> DownloadGameFile(string key)
    {
        try
        {
            if (User.HasReadOnlyAccess())
            {
                return Forbid("Guests cannot download game files");
            }

            _logger.LogInformation("Creating game file for game with key: {Key} for user: {User} (Role: {Role})",
                key, User.GetUserEmail(), User.GetUserRole());

            var filePath = await _gameService.CreateGameFileAsync(key);

            if (!System.IO.File.Exists(filePath))
            {
                return ResourceNotFound($"File for game '{key}' could not be found.");
            }

            var fileName = Path.GetFileName(filePath);
            var fileContent = await System.IO.File.ReadAllBytesAsync(filePath);

            _logger.LogInformation("Successfully created and sending game file for game key: {Key} to user: {User}",
                key, User.GetUserEmail());
            return File(fileContent, "application/json", fileName);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Game not found with key: {Key}", key);
            return ResourceNotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error creating game file for game key: {key}");
        }
    }

    /// <summary>
    /// Epic 9: Everyone can view games by publisher
    /// </summary>
    [HttpGet("{publisherName}/games")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<GameMetadataUpdateRequestDto>>> GetGamesByPublisherName(string publisherName)
    {
        try
        {
            _logger.LogInformation("Getting games by publisher name: {PublisherName}", publisherName);
            var games = await _publisherService.GetGamesByPublisherNameAsync(publisherName);

            if (games == null)
            {
                return ResourceNotFound($"Games with publisher name: '{publisherName}' not found.");
            }

            _logger.LogInformation("Successfully found games with publisher name: {PublisherName}", publisherName);
            return Ok(games);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error getting games by publisher name: {publisherName}");
        }
    }

    /// <summary>
    /// Epic 9: Admin can view deleted games
    /// </summary>
    [HttpGet("games/deleted")]
    [Authorize(Policy = "CanViewDeletedGames")]
    public Task<IActionResult> GetDeletedGames()
    {
        try
        {
            _logger.LogInformation("Getting deleted games for admin: {User}", User.GetUserEmail());

            // Mock implementation
            var deletedGames = new[]
            {
                new { Id = Guid.NewGuid(), Name = "Deleted Game 1", DeletedAt = DateTime.UtcNow.AddDays(-5) },
                new { Id = Guid.NewGuid(), Name = "Deleted Game 2", DeletedAt = DateTime.UtcNow.AddDays(-10) }
            };

            return Task.FromResult<IActionResult>(Ok(deletedGames));
        }
        catch (Exception ex)
        {
            return Task.FromResult<IActionResult>(HandleException(ex, "Error retrieving deleted games"));
        }
    }

    private ObjectResult InternalServerError(string message, string details = null)
    {
        _logger.LogWarning(message);

        return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponseModel
        {
            Message = message,
            Details = details,
            StatusCode = StatusCodes.Status500InternalServerError,
        });
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