using Gamestore.Entities.ErrorModels;
using Gamestore.Services.Dto.GamesDto;
using Gamestore.Services.Interfaces;
using Gamestore.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Gamestore.WebApi.Controllers.Business;

[ApiController]
[Route("api/games")]
public class GameController(
    IGameService gameService,
    IGenreService genreService,
    IPlatformService platformService,
    IPublisherService publisherService,
    ILogger<GameController> logger) : ControllerBase
{
    private readonly IGameService _gameService = gameService;
    private readonly IGenreService _genreService = genreService;
    private readonly IPlatformService _platformService = platformService;
    private readonly IPublisherService _publisherService = publisherService;
    private readonly ILogger<GameController> _logger = logger;

    /// <summary>
    /// Get all games with filters
    /// GET /api/games.
    /// </summary>
    [HttpGet]
    [OutputCache(Duration = 60)]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllGames()
    {
        try
        {
            _logger.LogInformation("Getting all games");
            var games = await _gameService.GetAllGames();
            _logger.LogInformation("Successfully retrieved all games");
            return Ok(games);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error retrieving all games");
        }
    }

    /// <summary>
    /// Get game by key
    /// GET /api/games/{key}.
    /// </summary>
    [HttpGet("{key}")]
    [OutputCache(Duration = 60)]
    [AllowAnonymous]
    public async Task<IActionResult> GetGameByKey(string key)
    {
        try
        {
            _logger.LogInformation("Getting game by key: {GameKey}", key);
            var game = await _gameService.GetGameByKey(key);

            if (game == null)
            {
                return ResourceNotFound($"Game with key '{key}' not found.");
            }

            _logger.LogInformation("Successfully retrieved game with key: {GameKey}", key);
            return Ok(game);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error retrieving game with key: {key}");
        }
    }

    /// <summary>
    /// Create new game
    /// POST /api/games.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "CanManageGames")]
    public async Task<IActionResult> CreateGame([FromBody] GameMetadataCreateRequestDto gameRequest)
    {
        try
        {
            if (gameRequest?.Game == null)
            {
                return BadRequest(new ErrorResponseModel
                {
                    Message = "Game data is required.",
                    StatusCode = StatusCodes.Status400BadRequest,
                });
            }

            _logger.LogInformation(
                "Creating game with key: {GameKey} by user: {User} (Role: {Role})",
                gameRequest.Game.Key,
                User.GetUserEmail(),
                User.GetUserRole());

            var newGame = await _gameService.AddGameAsync(gameRequest);

            if (newGame == null)
            {
                return InternalServerError("Failed to create the game.");
            }

            _logger.LogInformation(
                "Successfully created game with key: {GameKey} by user: {User}",
                newGame.Game.Key,
                User.GetUserEmail());
            return Ok(newGame);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error creating game");
        }
    }

    /// <summary>
    /// Update game
    /// PUT /api/games.
    /// </summary>
    [HttpPut]
    [Authorize(Policy = "CanManageGames")]
    public async Task<IActionResult> UpdateGame([FromBody] GameMetadataUpdateRequestDto gameUpdateDto)
    {
        try
        {
            if (gameUpdateDto?.Game == null || string.IsNullOrEmpty(gameUpdateDto.Game.Key))
            {
                return BadRequest(new ErrorResponseModel
                {
                    Message = "Invalid game data or missing Key.",
                    StatusCode = StatusCodes.Status400BadRequest,
                });
            }

            _logger.LogInformation(
                "Received game update request for key: {GameKey} from user: {User} (Role: {Role})",
                gameUpdateDto.Game.Key,
                User.GetUserEmail(),
                User.GetUserRole());

            var updatedGame = await _gameService.UpdateGameAsync(gameUpdateDto.Game.Key, gameUpdateDto);

            if (updatedGame == null)
            {
                return ResourceNotFound($"Game with key '{gameUpdateDto.Game.Key}' not found.");
            }

            _logger.LogInformation(
                "Successfully updated game with key: {GameKey} by user: {User}",
                gameUpdateDto.Game.Key,
                User.GetUserEmail());
            return Ok(updatedGame);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error updating game");
        }
    }

    /// <summary>
    /// Delete game by key
    /// DELETE /api/games/{key}.
    /// </summary>
    [HttpDelete("{key}")]
    [Authorize(Policy = "CanManageGames")]
    public async Task<IActionResult> DeleteGame(string key)
    {
        try
        {
            _logger.LogInformation(
                "Deleting game with key: {GameKey} by user: {User} (Role: {Role})",
                key,
                User.GetUserEmail(),
                User.GetUserRole());

            var deletedGame = await _gameService.DeleteGameAsync(key);

            if (deletedGame == null)
            {
                return ResourceNotFound($"Game with key '{key}' not found.");
            }

            _logger.LogInformation(
                "Successfully deleted game with key: {GameKey} by user: {User}",
                key,
                User.GetUserEmail());
            return Ok(deletedGame);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error deleting game with key: {key}");
        }
    }

    /// <summary>
    /// Get genres for a game
    /// GET /api/games/{key}/genres.
    /// </summary>
    [HttpGet("{key}/genres")]
    [OutputCache(Duration = 60)]
    [AllowAnonymous]
    public async Task<IActionResult> GetGameGenres(string key)
    {
        try
        {
            _logger.LogInformation("Getting genres for game with key: {GameKey}", key);
            var genres = await _genreService.GetGenresByGameKeyAsync(key);

            if (!genres.Any())
            {
                return ResourceNotFound($"No genres found for game '{key}'.");
            }

            _logger.LogInformation("Successfully retrieved genres for game with key: {GameKey}", key);
            return Ok(genres);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error retrieving genres for game with key: {key}");
        }
    }

    /// <summary>
    /// Get platforms for a game
    /// GET /api/games/{key}/platforms.
    /// </summary>
    [HttpGet("{key}/platforms")]
    [OutputCache(Duration = 60)]
    [AllowAnonymous]
    public async Task<IActionResult> GetGamePlatforms(string key)
    {
        try
        {
            _logger.LogInformation("Getting platforms for game with key: {GameKey}", key);
            var platforms = await _platformService.GetPlatformsByGameKeyAsync(key);

            if (!platforms.Any())
            {
                return ResourceNotFound($"No platforms found for game '{key}'.");
            }

            _logger.LogInformation("Successfully retrieved platforms for game with key: {GameKey}", key);
            return Ok(platforms);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error retrieving platforms for game with key: {key}");
        }
    }

    /// <summary>
    /// Get publisher for a game
    /// GET /api/games/{key}/publisher.
    /// </summary>
    [HttpGet("{key}/publisher")]
    [OutputCache(Duration = 60)]
    [AllowAnonymous]
    public async Task<IActionResult> GetGamePublisher(string key)
    {
        try
        {
            _logger.LogInformation("Getting publisher for game with key: {GameKey}", key);
            var publisher = await _publisherService.GetPublisherByGameKey(key);

            if (publisher == null)
            {
                return ResourceNotFound($"Publisher for game with key '{key}' not found.");
            }

            _logger.LogInformation("Successfully retrieved publisher for game with key: {GameKey}", key);
            return Ok(publisher);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error retrieving publisher for game with key: {key}");
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

    private ObjectResult InternalServerError(string message)
    {
        _logger.LogError(message);

        return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponseModel
        {
            Message = message,
            StatusCode = StatusCodes.Status500InternalServerError,
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