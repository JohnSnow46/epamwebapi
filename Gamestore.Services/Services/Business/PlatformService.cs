using System.ComponentModel.DataAnnotations;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Business;
using Gamestore.Services.Dto.PlatformsDto;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services.Services.Business;

/// <summary>
/// Service for managing platforms in the Gamestore.
/// </summary>
public class PlatformService(IUnitOfWork unitOfWork, ILogger<PlatformService> logger) : IPlatformService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<PlatformService> _logger = logger;

    #region Public Service methods

    /// <summary>
    /// Updates an existing platform.
    /// </summary>
    public async Task<PlatformMetadataUpdateRequestDto> UpdatePlatform(Guid id, PlatformMetadataUpdateRequestDto platformRequest)
    {
        _logger.LogInformation("Starting update platform operation for ID: {PlatformId}", id);

        var platformEntity = await GetRequiredPlatformById(id);

        await ValidatePlatformTypeForUpdate(platformEntity, platformRequest.Type);

        platformEntity.Type = platformRequest.Type ?? string.Empty;
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Platform updated with ID: {PlatformId}", platformEntity.Id);

        return new PlatformMetadataUpdateRequestDto
        {
            Id = platformEntity.Id,
            Type = platformEntity.Type,
        };
    }

    /// <summary>
    /// Creates a new platform.
    /// </summary>
    public async Task<PlatformCreateRequestDto> CreatePlatform(PlatformMetadataCreateRequestDto platformRequest)
    {
        _logger.LogInformation("Starting create platform operation");

        ValidateNotNull(platformRequest, nameof(platformRequest));
        ValidateNotNull(platformRequest.Platform, nameof(platformRequest.Platform));

        _logger.LogInformation("Validating uniqueness for new platform type: {PlatformType}", platformRequest.Platform.Type);
        await ValidatePlatformTypeUniqueness(platformRequest.Platform.Type ?? string.Empty);

        var platformEntity = CreatePlatformEntity(platformRequest.Platform);

        await _unitOfWork.Platforms.AddAsync(platformEntity);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("New platform created with ID: {PlatformId}", platformEntity.Id);

        return MapToPlatformDto(platformEntity);
    }

    /// <summary>
    /// Deletes a platform by its ID.
    /// </summary>
    public async Task<Platform> DeletePlatformById(Guid id)
    {
        _logger.LogInformation("Starting delete platform operation for ID: {PlatformId}", id);

        var platformEntity = await GetRequiredPlatformById(id);
        await ValidatePlatformCanBeDeleted(id);

        await _unitOfWork.Platforms.DeleteAsync(id);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Platform with ID: {PlatformId} deleted successfully", id);
        return platformEntity;
    }

    /// <summary>
    /// Gets all platforms.
    /// </summary>
    public async Task<IEnumerable<Platform>> GetAllPlatformsAsync()
    {
        _logger.LogInformation("Starting get all platforms operation");

        var platforms = await _unitOfWork.Platforms.GetAllAsync();
        var platformsList = platforms.ToList();

        _logger.LogInformation("Retrieved {Count} platforms from database", platformsList.Count);

        return platformsList;
    }

    /// <summary>
    /// Gets a platform by its ID.
    /// </summary>
    public async Task<Platform> GetPlatformById(Guid id)
    {
        _logger.LogInformation("Starting get platform by ID operation for ID: {PlatformId}", id);

        return await GetRequiredPlatformById(id);
    }

    /// <summary>
    /// Gets all platforms for a game by its key.
    /// </summary>
    public async Task<IEnumerable<Platform>> GetPlatformsByGameKeyAsync(string gameKey)
    {
        _logger.LogInformation("Starting get platforms by game key operation for key: {GameKey}", gameKey);

        ValidateGameKey(gameKey);
        var game = await GetGameByKeyAsync(gameKey);

        return await GetPlatformsByGameId(game.Id);
    }

    #endregion

    #region Private Helper Methods

    private static void ValidateNotNull<T>(T obj, string paramName)
        where T : class
    {
        if (obj == null)
        {
            throw new ArgumentNullException(paramName, $"{paramName} cannot be null");
        }
    }

    private async Task<Platform> GetRequiredPlatformById(Guid id)
    {
        ValidatePlatformId(id);

        var platform = await _unitOfWork.Platforms.GetByIdAsync(id);

        if (platform == null)
        {
            _logger.LogWarning("Platform not found for ID: {PlatformId}", id);
            throw new KeyNotFoundException($"Platform with ID '{id}' not found");
        }

        _logger.LogInformation("Platform found with ID: {PlatformId}", id);
        return platform;
    }

    private async Task ValidatePlatformTypeUniqueness(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ValidationException("Platform type cannot be empty");
        }

        var platforms = await _unitOfWork.Platforms.GetAllAsync();
        var exists = platforms.Any(p => string.Equals(p.Type, type, StringComparison.OrdinalIgnoreCase));

        if (exists)
        {
            _logger.LogWarning("Platform with type '{Type}' already exists", type);
            throw new ValidationException($"Platform with type '{type}' already exists");
        }
    }

    private async Task ValidatePlatformTypeForUpdate(Platform existingPlatform, string newType)
    {
        if (!string.Equals(existingPlatform.Type, newType, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Platform type changed from '{OldType}' to '{NewType}' - validating uniqueness", existingPlatform.Type, newType);

            await ValidatePlatformTypeUniqueness(newType ?? string.Empty);
        }
    }

    private async Task<bool> IsPlatformUsedByGames(Guid platformId)
    {
        var gamePlatforms = await _unitOfWork.GamePlatforms.GetByPlatformIdAsync(platformId);
        var isUsed = gamePlatforms != null && gamePlatforms.Any();

        _logger.LogInformation("Platform with ID: {PlatformId} is used by games: {IsUsed}", platformId, isUsed);

        return isUsed;
    }

    private void ValidatePlatformId(Guid id)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Provided platform ID is empty");
            throw new ArgumentException("Platform ID cannot be empty", nameof(id));
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

    private async Task ValidatePlatformCanBeDeleted(Guid platformId)
    {
        if (await IsPlatformUsedByGames(platformId))
        {
            _logger.LogWarning("Cannot delete platform with ID: {PlatformId} because it is used by games", platformId);
            throw new InvalidOperationException("Cannot delete a platform that is used by games. Please remove the platform from games first");
        }
    }

    private async Task<IEnumerable<Platform>> GetPlatformsByGameId(Guid gameId)
    {
        _logger.LogInformation("Retrieving platforms for game with ID: {GameId}", gameId);

        var gamePlatforms = await _unitOfWork.GamePlatforms.GetByGameIdAsync(gameId);

        if (gamePlatforms == null || gamePlatforms.Count == 0)
        {
            _logger.LogInformation("No platforms found for game with ID: {GameId}", gameId);
            throw new KeyNotFoundException($"No platforms found for game with ID '{gameId}'");
        }

        _logger.LogInformation("Found {Count} game-platform relations for game with ID: {GameId}", gamePlatforms.Count, gameId);

        return await GetPlatformsByIds(gamePlatforms.Select(gp => gp.PlatformId).ToList());
    }

    private async Task<IEnumerable<Platform>> GetPlatformsByIds(List<Guid> platformIds)
    {
        var platforms = await _unitOfWork.GamePlatforms.GetByIdsAsync(platformIds);
        _logger.LogInformation("Retrieved {Count} platforms for game relations", platforms.Count);
        return platforms;
    }

    private static Platform CreatePlatformEntity(PlatformCreateRequestDto platformDto)
    {
        return new Platform
        {
            Id = platformDto.Id != Guid.Empty ? platformDto.Id : Guid.NewGuid(),
            Type = platformDto.Type ?? string.Empty,
        };
    }

    private static PlatformCreateRequestDto MapToPlatformDto(Platform platform)
    {
        ArgumentNullException.ThrowIfNull(platform);

        return new PlatformCreateRequestDto
        {
            Id = platform.Id,
            Type = platform.Type,
        };
    }

    #endregion
}