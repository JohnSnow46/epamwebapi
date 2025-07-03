using System.Text.Json;
using Gamestore.Entities.ErrorModels;
using Gamestore.Services.Dto.GenresDto;
using Gamestore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Authorization;
using Gamestore.WebApi.Extensions;

namespace Gamestore.WebApi.Controllers.Business;

[ApiController]
[Route("api")]
public class GenreController(IGameService gameService, IGenreService genreService, ILogger<GenreController> logger) : ControllerBase
{
    private readonly IGameService _gameService = gameService;
    private readonly IGenreService _genreService = genreService;
    private readonly ILogger<GenreController> _logger = logger;

    /// <summary>
    /// Epic 9: Admin and Manager can create genres (business entities)
    /// </summary>
    [HttpPost("genres/add-genre")]
    [Authorize(Policy = "CanManageBusinessEntities")]
    public async Task<IActionResult> CreateGenre([FromBody] GenreMetadataCreateRequestDto genreRequest)
    {
        try
        {
            _logger.LogInformation("Creating new genre with Name: {GenreName} by user: {User} (Role: {Role})",
                genreRequest.Genre.Name, User.GetUserEmail(), User.GetUserRole());

            var newGenre = await _genreService.CreateGenre(genreRequest.Genre);
            _logger.LogInformation("Successfully created genre with Name: {GenreName} by user: {User}",
                newGenre.Name, User.GetUserEmail());
            return Ok(newGenre);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error creating genre");
        }
    }

    /// <summary>
    /// Epic 9: Admin and Manager can update genres (business entities)
    /// </summary>
    [HttpPut("genres/update-genre")]
    [Authorize(Policy = "CanManageBusinessEntities")]
    public async Task<IActionResult> UpdateGenre([FromBody] JsonElement requestData)
    {
        try
        {
            _logger.LogInformation("Received genre update request from user: {User} (Role: {Role})",
                User.GetUserEmail(), User.GetUserRole());

            if (!requestData.TryGetProperty("genre", out var genreElement))
            {
                return BadRequest(new ErrorResponseModel
                {
                    Message = "Invalid request format. Expected 'genre' property.",
                    StatusCode = StatusCodes.Status400BadRequest,
                });
            }

            var genreUpdateDto = genreElement.Deserialize<GenreMetadataUpdateRequestDto>();
            if (genreUpdateDto == null || genreUpdateDto.Id == Guid.Empty)
            {
                return BadRequest(new ErrorResponseModel
                {
                    Message = "Invalid genre data or missing ID.",
                    StatusCode = StatusCodes.Status400BadRequest,
                });
            }

            var id = genreUpdateDto.Id;
            _logger.LogInformation(
                "Updating genre with ID: {GenreId}, Name: {GenreName}, ParentId: {ParentId} by user: {User}",
                id,
                genreUpdateDto.Name,
                genreUpdateDto.ParentGenreId,
                User.GetUserEmail());

            var updatedGenre = await _genreService.UpdateGenre(id, genreUpdateDto);
            _logger.LogInformation("Successfully updated genre with ID: {GenreId} by user: {User}",
                updatedGenre.Id, User.GetUserEmail());
            return Ok(new { genre = updatedGenre });
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error updating genre");
        }
    }

    /// <summary>
    /// Epic 9: Everyone can view genres (read-only for guests)
    /// </summary>
    [HttpGet("genres/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetGenreById(Guid id)
    {
        try
        {
            var userInfo = User.Identity?.IsAuthenticated == true
                ? $"{User.GetUserEmail()} (Role: {User.GetUserRole()})"
                : "Anonymous Guest";

            _logger.LogInformation("Getting genre by ID: {GenreId} for user: {UserInfo}", id, userInfo);
            var genre = await _genreService.GetGenreById(id);

            if (genre == null)
            {
                return ResourceNotFound($"Genre with ID '{id}' not found.");
            }

            _logger.LogInformation("Successfully retrieved genre with ID: {GenreId}", id);
            return Ok(genre);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error getting genre by ID: {id}");
        }
    }

    /// <summary>
    /// Epic 9: Everyone can view all genres
    /// </summary>
    [HttpGet("genres/")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllGenres()
    {
        try
        {
            _logger.LogInformation("Getting all genres");
            var genres = await _genreService.GetAllGenres();
            _logger.LogInformation("Successfully retrieved {Count} genres", genres.Count());
            return Ok(genres);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error retrieving all genres");
        }
    }

    /// <summary>
    /// Epic 9: Everyone can view subgenres
    /// </summary>
    [HttpGet("genres/{id}/subgenres")]
    [AllowAnonymous]
    public async Task<IActionResult> GetGenresByParentId(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting subgenres for parent genre ID: {ParentId}", id);
            var subGenres = await _genreService.GetSubGenresAsync(id);
            _logger.LogInformation("Successfully retrieved {Count} subgenres for parent genre ID: {ParentId}",
                subGenres.Count(), id);
            return Ok(subGenres);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error retrieving subgenres for parent genre ID: {id}");
        }
    }

    /// <summary>
    /// Epic 9: Admin and Manager can delete genres (business entities)
    /// </summary>
    [HttpDelete("genres/{id}")]
    [Authorize(Policy = "CanManageBusinessEntities")]
    public async Task<IActionResult> DeleteGenreById(Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting genre with ID: {GenreId} by user: {User} (Role: {Role})",
                id, User.GetUserEmail(), User.GetUserRole());

            var genre = await _genreService.DeleteGenreById(id);

            if (genre == null)
            {
                return ResourceNotFound($"Genre with ID '{id}' not found.");
            }

            _logger.LogInformation("Successfully deleted genre with ID: {GenreId} by user: {User}",
                id, User.GetUserEmail());
            return Ok(genre);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error deleting genre with ID: {id}");
        }
    }

    /// <summary>
    /// Epic 9: Everyone can view genres by game key (cached for performance)
    /// </summary>
    [HttpGet("genres/{key}/genres")]
    [OutputCache(Duration = 60)]
    [AllowAnonymous]
    public async Task<IActionResult> GetGenresByGameKey(string key)
    {
        try
        {
            _logger.LogInformation("Getting genres for game with key: {GameKey}", key);
            var genres = await _genreService.GetGenresByGameKeyAsync(key);

            if (!genres.Any())
            {
                return ResourceNotFound($"No genres found for game '{key}'.");
            }

            _logger.LogInformation("Successfully retrieved {Count} genres for game with key: {GameKey}",
                genres.Count(), key);
            return Ok(genres);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error retrieving genres for game with key: {key}");
        }
    }

    /// <summary>
    /// Epic 9: Everyone can view games by genre
    /// </summary>
    [HttpGet("genres/{id}/games")]
    [AllowAnonymous]
    public async Task<IActionResult> GetGamesByGenreId(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting games by genre ID: {GenreId}", id);
            var games = await _gameService.GetGamesByGenreAsync(id);

            if (games == null)
            {
                return ResourceNotFound($"Genre with ID '{id}' not found.");
            }

            _logger.LogInformation("Successfully retrieved {Count} games for genre ID: {GenreId}",
                games.Count(), id);
            return Ok(games);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error retrieving games for genre ID: {id}");
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