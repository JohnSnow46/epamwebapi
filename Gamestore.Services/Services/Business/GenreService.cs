using System.ComponentModel.DataAnnotations;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Business;
using Gamestore.Services.Dto.GenresDto;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services.Services.Business;

/// <summary>
/// Service for managing genres in the Gamestore.
/// </summary>
public class GenreService(IUnitOfWork unitOfWork, ILogger<GenreService> logger) : IGenreService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<GenreService> _logger = logger;

    #region Public Genre methods

    /// <summary>
    /// Creates a new genre.
    /// </summary>
    public async Task<GenreCreateRequestDto> CreateGenre(GenreCreateRequestDto genreRequest)
    {
        _logger.LogInformation("Starting create genre operation for name: {GenreName}", genreRequest?.Name);

        ValidateNotNull(genreRequest, nameof(genreRequest));
        await ValidateGenreNameUniqueness(genreRequest.Name ?? string.Empty);

        var genreEntity = CreateGenreEntityFromDto(genreRequest);

        if (genreRequest.ParentGenreId.HasValue)
        {
            await ValidateParentGenreExists(genreRequest.ParentGenreId.Value);
        }

        await _unitOfWork.Genres.AddAsync(genreEntity);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Successfully saved new genre with ID: {GenreId}", genreEntity.Id);

        return new GenreCreateRequestDto
        {
            Name = genreEntity.Name,
            ParentGenreId = genreEntity.ParentGenreId,
        };
    }

    /// <summary>
    /// Updates an existing genre.
    /// </summary>
    public async Task<GenreUpdateRequestDto> UpdateGenre(Guid id, GenreMetadataUpdateRequestDto genreRequest)
    {

        _logger.LogInformation("Starting update genre operation for ID: {GenreId}", id);

        ValidateNotNull(genreRequest, nameof(genreRequest));

        var genreEntity = await GetRequiredGenreById(id);

        await ValidateGenreNameForUpdate(genreEntity, genreRequest.Name);

        if (genreRequest.ParentGenreId.HasValue)
        {
            await ValidateGenreHierarchy(id, genreRequest.ParentGenreId.Value);
        }

        UpdateGenreFromDto(genreEntity, genreRequest);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Successfully updated genre with ID: {GenreId}", genreEntity.Id);

        return MapToGenreDto(genreEntity);
    }

    /// <summary>
    /// Gets a genre by its ID.
    /// </summary>
    public async Task<GenreUpdateRequestDto> GetGenreById(Guid id)
    {
        _logger.LogInformation("Starting get genre by ID operation for ID: {GenreId}", id);

        var genre = await GetRequiredGenreById(id);
        return MapToGenreDto(genre);
    }

    /// <summary>
    /// Gets all genres.
    /// </summary>
    public async Task<IEnumerable<GenreUpdateRequestDto>> GetAllGenres()
    {
        _logger.LogInformation("Starting get all genres operation");

        var genres = await _unitOfWork.Genres.GetAllAsync();
        var genresList = genres.ToList();

        _logger.LogInformation("Retrieved {Count} genres from database", genresList.Count);

        return genresList.Select(MapToGenreDto);
    }

    /// <summary>
    /// Gets all sub-genres for a given genre.
    /// </summary>
    public async Task<IEnumerable<GenreUpdateRequestDto>> GetSubGenresAsync(Guid id)
    {
        _logger.LogInformation("Starting get sub-genres operation for genre ID: {GenreId}", id);

        await GetRequiredGenreById(id);
        return await GetSubGenresByParentId(id);
    }

    /// <summary>
    /// Deletes a genre by its ID.
    /// </summary>
    public async Task<GenreUpdateRequestDto> DeleteGenreById(Guid id)
    {
        _logger.LogInformation("Starting delete genre operation for ID: {GenreId}", id);

        var genre = await GetRequiredGenreById(id);
        await ValidateGenreCanBeDeleted(id);

        await _unitOfWork.Genres.DeleteAsync(id);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Successfully deleted genre with ID: {GenreId}", id);

        return MapToGenreDto(genre);
    }

    /// <summary>
    /// Gets all genres for a game by its key.
    /// </summary>
    public async Task<IEnumerable<GenreUpdateRequestDto>> GetGenresByGameKeyAsync(string gameKey)
    {
        _logger.LogInformation("Starting get genres by game key operation for key: {GameKey}", gameKey);

        ValidateGameKey(gameKey);
        var game = await GetGameByKeyAsync(gameKey);
        return await GetGenresByGameId(game.Id);
    }

    #endregion

    #region Private Helper methods

    private async Task<Genre> GetRequiredGenreById(Guid id)
    {
        ValidateGuid(id, "Genre ID");

        _logger.LogInformation("Getting genre by ID: {GenreId}", id);

        var genre = await _unitOfWork.Genres.GetByIdAsync(id);
        return genre;
    }

    private void ValidateGuid(Guid id, string paramName)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Provided {ParamName} is empty", paramName);
            throw new ArgumentException($"{paramName} cannot be empty", paramName.ToLowerInvariant());
        }
    }

    private void ValidateGameKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("Provided game key is null or empty");
            throw new ArgumentException("Game key cannot be null or empty", nameof(key));
        }
    }

    private async Task ValidateGenreNameUniqueness(string name)
    {
        _logger.LogInformation("Validating uniqueness for genre name: {GenreName}", name);
        IsNullChecker(name);

        var genres = await _unitOfWork.Genres.GetAllAsync();
        var exists = genres.Any(g => string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase));

        if (exists)
        {
            _logger.LogWarning("Genre with name '{GenreName}' already exists", name);
            throw new ValidationException($"Genre with name '{name}' already exists");
        }
    }

    private static void IsNullChecker(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Genre name cannot be null or empty.", nameof(name));
        }
    }

    private async Task ValidateGenreNameForUpdate(Genre existingGenre, string? newName)
    {
        if (!string.Equals(existingGenre.Name, newName, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Genre name changed from '{OldName}' to '{NewName}' - validating uniqueness", existingGenre.Name, newName);

            await ValidateGenreNameUniqueness(newName ?? string.Empty);
        }
    }

    private async Task ValidateParentGenreExists(Guid parentId)
    {
        try
        {
            await GetRequiredGenreById(parentId);
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("Parent genre with ID {ParentId} does not exist", parentId);
            throw new KeyNotFoundException($"Parent genre with ID {parentId} not found");
        }
    }

    private async Task ValidateGenreHierarchy(Guid genreId, Guid parentGenreId)
    {
        _logger.LogInformation("Validating genre hierarchy for genre ID: {GenreId} with parent ID: {ParentId}", genreId, parentGenreId);

        await ValidateParentGenreExists(parentGenreId);

        if (genreId == parentGenreId)
        {
            _logger.LogWarning("Genre cannot be its own parent - Genre ID: {GenreId}", genreId);
            throw new ValidationException("A genre cannot be its own parent");
        }

        await CheckForCircularReference(genreId, parentGenreId);
    }

    private async Task CheckForCircularReference(Guid genreId, Guid startParentId)
    {
        var currentParentId = startParentId;
        var visitedIds = new HashSet<Guid> { genreId };

        _logger.LogInformation("Checking for circular references in genre hierarchy");

        while (currentParentId != Guid.Empty && !visitedIds.Contains(currentParentId))
        {
            visitedIds.Add(currentParentId);

            try
            {
                var parent = await _unitOfWork.Genres.GetByIdAsync(currentParentId);
                currentParentId = parent.ParentGenreId ?? Guid.Empty;

                if (currentParentId == genreId)
                {
                    _logger.LogWarning("Circular reference detected in genre hierarchy for genre ID: {GenreId}", genreId);
                    throw new ValidationException("Circular reference detected in genre hierarchy");
                }
            }
            catch (KeyNotFoundException)
            {
                break;
            }
        }
    }

    private async Task ValidateGenreCanBeDeleted(Guid id)
    {
        _logger.LogInformation("Validating if genre can be deleted for ID: {GenreId}", id);

        if (await HasSubGenres(id))
        {
            _logger.LogWarning("Cannot delete genre with ID: {GenreId} because it has sub-genres", id);
            throw new InvalidOperationException("Cannot delete a genre that has sub-genres. Please delete the sub-genres first");
        }

        if (await IsGenreUsedByGames(id))
        {
            _logger.LogWarning("Cannot delete genre with ID: {GenreId} because it is used by games", id);
            throw new InvalidOperationException("Cannot delete a genre that is used by games. Please remove the genre from games first");
        }
    }

    private static void ValidateNotNull<T>(T? obj, string paramName)
        where T : class
    {
        if (obj == null)
        {
            throw new ArgumentNullException(paramName, $"{paramName} cannot be null");
        }
    }

    private async Task<bool> HasSubGenres(Guid genreId)
    {
        _logger.LogInformation("Checking if genre has sub-genres for ID: {GenreId}", genreId);

        var genres = await _unitOfWork.Genres.GetAllAsync();
        var hasSubGenres = genres.Any(g => g.ParentGenreId == genreId);

        _logger.LogInformation("Genre with ID: {GenreId} has sub-genres: {HasSubGenres}", genreId, hasSubGenres);

        return hasSubGenres;
    }

    private async Task<bool> IsGenreUsedByGames(Guid genreId)
    {
        _logger.LogInformation("Checking if genre is used by games for ID: {GenreId}", genreId);

        var gameGenres = await _unitOfWork.GameGenres.GetByGenreIdAsync(genreId);
        var isUsed = gameGenres != null && gameGenres.Any();

        _logger.LogInformation("Genre with ID: {GenreId} is used by games: {IsUsed}", genreId, isUsed);

        return isUsed;
    }

    private async Task<Game> GetGameByKeyAsync(string key)
    {
        var game = await _unitOfWork.Games.GetKeyAsync(key);

        if (game == null)
        {
            _logger.LogWarning("Game not found for key: {GameKey}", key);
            throw new KeyNotFoundException($"Game with key '{key}' not found");
        }

        _logger.LogInformation("Game found with ID: {GameId} for key: {GameKey}", game.Id, key);

        return game;
    }

    private async Task<IEnumerable<GenreUpdateRequestDto>> GetGenresByGameId(Guid gameId)
    {
        _logger.LogInformation("Retrieving genres for game with ID: {GameId}", gameId);

        var gameGenres = await _unitOfWork.GameGenres.GetByGameIdAsync(gameId);

        if (gameGenres == null || gameGenres.Count == 0)
        {
            _logger.LogInformation("No genres found for game with ID: {GameId}", gameId);
            throw new KeyNotFoundException($"No genres found for game with ID '{gameId}'");
        }

        _logger.LogInformation("Found {Count} game-genre relations for game with ID: {GameId}", gameGenres.Count, gameId);

        return await GetGenresByIds(gameGenres.Select(gg => gg.GenreId).ToList());
    }

    private async Task<IEnumerable<GenreUpdateRequestDto>> GetGenresByIds(List<Guid> genreIds)
    {
        var genres = await _unitOfWork.GameGenres.GetByIdsAsync(genreIds);
        _logger.LogInformation("Retrieved {Count} genres for game relations", genres.Count);
        return genres.Select(MapToGenreDto);
    }

    private async Task<IEnumerable<GenreUpdateRequestDto>> GetSubGenresByParentId(Guid parentId)
    {
        _logger.LogInformation("Retrieving sub-genres for parent genre ID: {GenreId}", parentId);

        var allGenres = await _unitOfWork.Genres.GetAllAsync();
        var subGenres = allGenres.Where(g => g.ParentGenreId == parentId).ToList();

        _logger.LogInformation("Found {Count} sub-genres for parent genre ID: {GenreId}", subGenres.Count, parentId);

        return subGenres.Select(MapToGenreDto);
    }

    private static Genre CreateGenreEntityFromDto(GenreCreateRequestDto genreDto)
    {
        ArgumentNullException.ThrowIfNull(genreDto);

        return string.IsNullOrWhiteSpace(genreDto.Name)
            ? throw new ArgumentException("Genre name cannot be null or empty.", nameof(genreDto))
            : new Genre
            {
                Id = Guid.NewGuid(),
                Name = genreDto.Name,
                ParentGenreId = genreDto.ParentGenreId,
            };
    }

    private static void UpdateGenreFromDto(Genre genreEntity, GenreMetadataUpdateRequestDto genreDto)
    {
        ArgumentNullException.ThrowIfNull(genreEntity);
        ArgumentNullException.ThrowIfNull(genreDto);

        if (string.IsNullOrWhiteSpace(genreDto.Name))
        {
            throw new ArgumentException("Genre name cannot be null or empty.", nameof(genreDto));
        }

        genreEntity.Name = genreDto.Name;
        genreEntity.ParentGenreId = genreDto.ParentGenreId;
    }

    private static GenreUpdateRequestDto MapToGenreDto(Genre genreEntity)
    {
        ArgumentNullException.ThrowIfNull(genreEntity);

        return new GenreUpdateRequestDto
        {
            Id = genreEntity.Id,
            Name = genreEntity.Name,
            ParentGenreId = genreEntity.ParentGenreId,
        };
    }

    #endregion
}