using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Business;
using Gamestore.Services.Caching;
using Gamestore.Services.Dto.GamesDto;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Gamestore.Services.Services.Business;

public partial class GameService(
    IUnitOfWork unitOfWork,
    ILogger<GameService> logger,
    ICacheService cacheService,
    IBlobStorageService blobStorageService) : IGameService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<GameService> _logger = logger;
    private readonly IBlobStorageService _blobStorageService = blobStorageService;
    private readonly ICacheService _cacheService = cacheService;

    /// <summary>
    /// Adds a new game with associated genres and platforms.
    /// Epic 10 US1: Now supports image upload in base64 format.
    /// </summary>
    public async Task<GameMetadataCreateRequestDto?> AddGameAsync(GameMetadataCreateRequestDto gameRequest)
    {
        _logger.LogInformation("Starting add game operation for game: {GameName}", gameRequest.Game?.Name);

        ValidateObject(gameRequest, "Game request");

        if (gameRequest.Game == null)
        {
            throw new ArgumentNullException(nameof(gameRequest.Game), "Game data cannot be null");
        }

        if (string.IsNullOrWhiteSpace(gameRequest.Game.Key))
        {
            ValidateString(gameRequest.Game.Name, "Game name");
            gameRequest.Game.Key = GenerateKeyFromName(gameRequest.Game.Name);
            _logger.LogInformation(
                "Key was not provided, generated key: {Key} from name: {Name}",
                gameRequest.Game.Key,
                gameRequest.Game.Name);
        }

        await ValidateGameKeyUniqueness(gameRequest.Game.Key!);

        var newGame = CreateGameEntity(gameRequest);
        await _unitOfWork.Games.AddAsync(newGame);
        _logger.LogInformation("New game created with ID: {GameId}", newGame.Id);

        await AddGamePlatforms(newGame.Id, gameRequest.Platforms);
        await AddGameGenres(newGame.Id, gameRequest.Genres);

        if (!string.IsNullOrEmpty(gameRequest.Image))
        {
            try
            {
                await _blobStorageService.UploadImageAsync(gameRequest.Image, gameRequest.Game.Key);
                _logger.LogInformation("Successfully uploaded image for game: {GameKey}", gameRequest.Game.Key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload image for game: {GameKey}. Game will be created without image.", gameRequest.Game.Key);
            }
        }

        await _unitOfWork.CompleteAsync();

        // INVALIDATE CACHE
        _logger.LogInformation("Invalidating cache after creating game");
        _cacheService.RemoveMultiple(
            CacheKeys.AllGames,
            CacheKeys.AllGamesCount);

        return gameRequest;
    }

    /// <summary>
    /// Updates an existing game with associated genres and platforms.
    /// Epic 10 US2: Now supports image upload in base64 format.
    /// </summary>
    public async Task<GameUpdateRequestDto> UpdateGameAsync(
        string key,
        GameMetadataUpdateRequestDto gameRequest)
    {
        _logger.LogInformation("Starting update game operation for key: {GameKey}", key);
        ValidateString(key, "Game key");

        if (gameRequest?.Game == null)
        {
            throw new ArgumentNullException(nameof(gameRequest.Game), "Game data cannot be null");
        }

        var existingGame = await GetGameByKeyOrNull(key);
        if (existingGame == null)
        {
            return null;
        }

        UpdateGameFromDto(existingGame, gameRequest);
        await UpdateGameRelations(existingGame.Id, gameRequest);

        if (!string.IsNullOrEmpty(gameRequest.Image))
        {
            try
            {
                var oldBlobName = _blobStorageService.GetBlobNameFromGameKey(key);
                var oldImageExists = await _blobStorageService.ImageExistsAsync(oldBlobName);

                if (oldImageExists)
                {
                    await _blobStorageService.DeleteImageAsync(oldBlobName);
                    _logger.LogInformation("Deleted old image for game: {GameKey}", key);
                }

                var newImageUrl = await _blobStorageService.UploadImageAsync(
                    gameRequest.Image,
                    key);
                existingGame.ImageUrl = newImageUrl;
                _logger.LogInformation("Uploaded new image for game: {GameKey}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update image for game: {GameKey}. Game will be updated without new image.", key);
            }
        }

        await _unitOfWork.CompleteAsync();
        _logger.LogInformation("Game updated successfully with ID: {GameId}", existingGame.Id);

        // INVALIDATE CACHE
        _logger.LogInformation("Invalidating cache after updating game '{GameKey}'", key);
        _cacheService.RemoveMultiple(
            CacheKeys.AllGames,
            CacheKeys.GameByKey(key),
            CacheKeys.GameById(existingGame.Id));

        // Invalidate genre cache
        foreach (var genre in existingGame.GameGenres ?? Enumerable.Empty<GameGenre>())
        {
            _cacheService.Remove(CacheKeys.GamesByGenre(genre.GenreId));
        }

        // Invalidate platform cache
        foreach (var platform in existingGame.GamePlatforms ?? Enumerable.Empty<GamePlatform>())
        {
            _cacheService.Remove(CacheKeys.GamesByPlatform(platform.PlatformId));
        }

        return new GameUpdateRequestDto
        {
            Id = existingGame.Id,
            Name = existingGame.Name,
            Key = existingGame.Key,
            Description = existingGame.Description,
            Price = existingGame.Price,
            UnitInStock = existingGame.UnitInStock,
            Discontinued = existingGame.Discontinued,
            Image = existingGame.ImageUrl,
        };
    }

    /// <summary>
    /// Gets a game by its key.
    /// </summary>
    public async Task<GameUpdateRequestDto?> GetGameByKey(string key)
    {
        _logger.LogInformation("Starting get game operation by key: {GameKey}", key);

        ValidateString(key, "Game key");

        // CHECK CACHE
        string cacheKey = CacheKeys.GameByKey(key);
        if (_cacheService.TryGetValue(cacheKey, out GameUpdateRequestDto cachedGame))
        {
            _logger.LogInformation("Retrieved game '{GameKey}' from cache", key);
            return cachedGame;
        }

        var existingGame = await GetGameByKeyOrNull(key);

        if (existingGame == null)
        {
            return null;
        }

        var gameDetails = MapToGameDtoUpdate(existingGame);
        _logger.LogInformation("Successfully retrieved game with ID: {GameId}", existingGame.Id);

        // CACHE RESULT
        _cacheService.Set(
            cacheKey,
            gameDetails,
            CacheKeys.SingleGameCacheDuration);

        return gameDetails;
    }

    /// <summary>
    /// Gets a game by its ID.
    /// </summary>
    public async Task<GameCreateRequestDto> GetGameById(Guid id)
    {
        _logger.LogInformation("Starting get game operation by ID: {GameId}", id);

        ValidateGuid(id, "Game ID");
        var game = await GetGameByIdOrThrow(id);
        return MapToGameDto(game);
    }

    /// <summary>
    /// Deletes a game by its key.
    /// Epic 10 US3: Now removes associated image from blob storage.
    /// </summary>
    public async Task<Game> DeleteGameAsync(string key)
    {
        _logger.LogInformation("Starting deletion process for game with key: {Key}", key);

        ValidateString(key, "Game key");

        var game = await GetGameByKeyOrNull(key) ?? throw new KeyNotFoundException($"Game with key '{key}' not found");

        try
        {
            var blobName = _blobStorageService.GetBlobNameFromGameKey(key);
            if (await _blobStorageService.ImageExistsAsync(blobName))
            {
                await _blobStorageService.DeleteImageAsync(blobName);
                _logger.LogInformation("Successfully deleted image for game: {GameKey}", key);
            }
            else
            {
                _logger.LogInformation("No image found for game: {GameKey}, skipping image deletion", key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete image for game: {GameKey}. Continuing with game deletion.", key);
        }

        await RemoveGameRelations(game);
        await _unitOfWork.Games.DeleteGameByKey(game);
        await _unitOfWork.CompleteAsync();

        // INVALIDATE CACHE
        _logger.LogInformation("Invalidating cache after deleting game '{GameKey}'", key);
        _cacheService.RemoveMultiple(
            CacheKeys.AllGames,
            CacheKeys.AllGamesCount,
            CacheKeys.GameByKey(key),
            CacheKeys.GameById(game.Id));

        // Invalidate genre cache
        foreach (var genre in game.GameGenres ?? Enumerable.Empty<GameGenre>())
        {
            _cacheService.Remove(CacheKeys.GamesByGenre(genre.GenreId));
        }

        // Invalidate platform cache
        foreach (var platform in game.GamePlatforms ?? Enumerable.Empty<GamePlatform>())
        {
            _cacheService.Remove(CacheKeys.GamesByPlatform(platform.PlatformId));
        }

        // Invalidate publisher cache
        if (game.PublisherId.HasValue)
        {
            _cacheService.Remove(CacheKeys.GamesByPublisher(game.PublisherId.Value));
        }

        _logger.LogInformation("Successfully deleted game with key: {Key}, ID: {Id}", key, game.Id);
        return game;
    }

    /// <summary>
    /// Gets all games.
    /// </summary>
    public async Task<IEnumerable<GameCreateRequestDto>> GetAllGames()
    {
        _logger.LogInformation("Starting get all games operation");

        // CHECK CACHE
        if (_cacheService.TryGetValue(CacheKeys.AllGames, out IEnumerable<GameCreateRequestDto> cachedGames))
        {
            _logger.LogInformation("Retrieved all games from cache");
            return cachedGames;
        }

        // FETCH FROM DATABASE
        var games = await _unitOfWork.Games.GetAllAsync();
        var gameList = games.ToList();

        _logger.LogInformation("Retrieved {Count} games from database", gameList.Count);
        var gameDtos = gameList.Select(MapToGameDto).ToList();

        // CACHE RESULT
        _cacheService.Set(
            CacheKeys.AllGames,
            gameDtos,
            CacheKeys.GamesCacheDuration);

        return gameDtos;
    }

    /// <summary>
    /// Gets image for game (NO cache - always fresh from Azure!).
    /// </summary>
    public async Task<byte[]?> GetGameImageAsync(string gameKey)
    {
        try
        {
            ValidateString(gameKey, "Game key");

            _logger.LogInformation("Retrieving image for game: {GameKey}", gameKey);

            var blobName = _blobStorageService.GetBlobNameFromGameKey(gameKey);
            var imageExists = await _blobStorageService.ImageExistsAsync(blobName);
            if (!imageExists)
            {
                _logger.LogWarning("Image not found for game: {GameKey}", gameKey);
                return null;
            }

            var imageBytes = await _blobStorageService.GetImageAsync(blobName);

            if (imageBytes != null && imageBytes.Length > 0)
            {
                _logger.LogInformation(
                    "Successfully retrieved image for game: {GameKey} ({SizeKB}KB)",
                    gameKey,
                    imageBytes.Length / 1024);
            }

            return imageBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving image for game: {GameKey}", gameKey);
            throw;
        }
    }

    /// <summary>
    /// Creates a file with serialized game data using the game key.
    /// </summary>
    public async Task<string> CreateGameFileAsync(string gameKey)
    {
        _logger.LogInformation("Starting create game file operation for key: {GameKey}", gameKey);

        ValidateString(gameKey, "Game key");

        var game = await GetGameByKeyOrNull(gameKey) ?? throw new KeyNotFoundException($"Game with key '{gameKey}' not found");
        var filePath = GenerateGameFilePath(game);
        var json = JsonConvert.SerializeObject(game, Formatting.Indented);

        await SaveGameFile(filePath, json);

        _logger.LogInformation("Successfully created game file at path: {FilePath}", filePath);
        return filePath;
    }

    /// <summary>
    /// Gets games filtered by platform ID.
    /// </summary>
    public async Task<IEnumerable<GameCreateRequestDto>> GetGamesByPlatformAsync(Guid platformId)
    {
        _logger.LogInformation("Starting get games by platform operation for platform ID: {PlatformId}", platformId);

        ValidateGuid(platformId, "Platform ID");

        var gamePlatforms = await GetGamePlatformRelationsAsync(platformId);
        var gameIds = gamePlatforms.Select(gp => gp.GameId).ToList();

        var games = await GetGamesByRelations(gameIds);

        _logger.LogInformation("Retrieved {Count} games for platform ID: {PlatformId}", games.Count(), platformId);
        return games;
    }

    /// <summary>
    /// Gets games filtered by genre ID.
    /// </summary>
    public async Task<IEnumerable<GameCreateRequestDto>> GetGamesByGenreAsync(Guid genreId)
    {
        _logger.LogInformation("Starting get games by genre operation for genre ID: {GenreId}", genreId);

        ValidateGuid(genreId, "Genre ID");

        var gameGenres = await GetGameGenreRelationsAsync(genreId);
        var gameIds = gameGenres.Select(gg => gg.GameId).ToList();

        var games = await GetGamesByRelations(gameIds);

        _logger.LogInformation("Retrieved {Count} games for genre ID: {GenreId}", games.Count(), genreId);
        return games;
    }

    /// <summary>
    /// Gets the total count of games in the database.
    /// </summary>
    public async Task<int> GetTotalGamesCountAsync()
    {
        _logger.LogInformation("Getting total games count");

        var games = await _unitOfWork.Games.GetAllAsync();
        var count = games.Count();

        _logger.LogInformation("Total games count: {Count}", count);
        return count;
    }

    private async Task AddGameGenres(Guid gameId, List<Guid>? genreIds)
    {
        if (genreIds == null || genreIds.Count == 0)
        {
            return;
        }

        var validGenreIds = FilterValidGuids(genreIds);
        if (validGenreIds.Count == 0)
        {
            return;
        }

        var gameGenres = CreateGameGenreEntities(gameId, validGenreIds);
        await _unitOfWork.GameGenres.AddRangeAsync(gameGenres);

        _logger.LogInformation("Added {Count} genres to game: {GameId}", gameGenres.Count, gameId);
    }

    private async Task AddGamePlatforms(Guid gameId, List<Guid>? platformIds)
    {
        if (platformIds == null || platformIds.Count == 0)
        {
            return;
        }

        var validPlatformIds = FilterValidGuids(platformIds);
        if (validPlatformIds.Count == 0)
        {
            return;
        }

        var gamePlatforms = CreateGamePlatformEntities(gameId, validPlatformIds);
        await _unitOfWork.GamePlatforms.AddRangeAsync(gamePlatforms);

        _logger.LogInformation("Added {Count} platforms to game: {GameId}", gamePlatforms.Count, gameId);
    }

    private async Task UpdateGameGenres(Guid gameId, List<Guid>? genreIds)
    {
        var existingGenres = await _unitOfWork.GameGenres.GetByGameIdAsync(gameId);
        if (existingGenres.Count != 0)
        {
            await _unitOfWork.GameGenres.RemoveRangeAsync(existingGenres);
            _logger.LogInformation(
                "Removed {Count} existing genres from game: {GameId}",
                existingGenres.Count,
                gameId);
        }

        await AddGameGenres(gameId, genreIds);
    }

    private async Task UpdateGamePlatforms(Guid gameId, List<Guid>? platformIds)
    {
        var existingPlatforms = await _unitOfWork.GamePlatforms.GetByGameIdAsync(gameId);
        if (existingPlatforms.Count != 0)
        {
            await _unitOfWork.GamePlatforms.RemoveRangeAsync(existingPlatforms);
            _logger.LogInformation(
                "Removed {Count} existing platforms from game: {GameId}",
                existingPlatforms.Count,
                gameId);
        }

        await AddGamePlatforms(gameId, platformIds);
    }

    private async Task UpdateGameRelations(Guid gameId, GameMetadataUpdateRequestDto gameRequest)
    {
        await UpdateGameGenres(gameId, gameRequest.Genres);
        await UpdateGamePlatforms(gameId, gameRequest.Platforms);
    }

    private async Task RemoveGameRelations(Game game)
    {
        if (game.GameGenres?.Count > 0)
        {
            _logger.LogInformation(
                "Removing {Count} genre associations for game ID: {GameId}",
                game.GameGenres.Count,
                game.Id);
            await _unitOfWork.GameGenres.RemoveRangeAsync(game.GameGenres);
        }

        if (game.GamePlatforms?.Count > 0)
        {
            _logger.LogInformation(
                "Removing {Count} platform associations for game ID: {GameId}",
                game.GamePlatforms.Count,
                game.Id);
            await _unitOfWork.GamePlatforms.RemoveRangeAsync(game.GamePlatforms);
        }
    }

    private static List<Guid> FilterValidGuids(List<Guid> ids)
    {
        return ids.Where(id => id != Guid.Empty).ToList();
    }

    private static List<GameGenre> CreateGameGenreEntities(Guid gameId, List<Guid> genreIds)
    {
        return genreIds.Select(genreId => new GameGenre
        {
            GameId = gameId,
            GenreId = genreId,
        }).ToList();
    }

    private static List<GamePlatform> CreateGamePlatformEntities(Guid gameId, List<Guid> platformIds)
    {
        return platformIds.Select(platformId => new GamePlatform
        {
            GameId = gameId,
            PlatformId = platformId,
        }).ToList();
    }

    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return "unnamed";
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

        return sanitized.Length > 50 ? sanitized[..50] : sanitized;
    }

    private async Task ValidateGameKeyUniqueness(string key)
    {
        ValidateString(key, "Game key");

        var game = await _unitOfWork.Games.GetKeyAsync(key);
        if (game != null)
        {
            _logger.LogWarning("Game with key '{Key}' already exists", key);
            throw new ValidationException($"Game with key '{key}' already exists");
        }
    }

    private async Task<Game> GetGameByIdOrThrow(Guid id)
    {
        try
        {
            var game = await _unitOfWork.Games.GetByIdAsync(id);
            _logger.LogInformation("Game found with ID: {GameId}", id);
            return game;
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("Game not found with ID: {GameId}", id);
            throw;
        }
    }

    private async Task<Game?> GetGameByKeyOrNull(string key)
    {
        var game = await _unitOfWork.Games.GetKeyAsync(key);

        if (game == null)
        {
            _logger.LogWarning("Game with key: {GameKey} not found", key);
        }
        else
        {
            _logger.LogInformation("Game found with key: {GameKey}, ID: {GameId}", key, game.Id);
        }

        return game;
    }

    private async Task<IEnumerable<GamePlatform>> GetGamePlatformRelationsAsync(Guid platformId)
    {
        var gamePlatforms = await _unitOfWork.GamePlatforms.GetByPlatformIdAsync(platformId);
        _logger.LogInformation(
            "Found {Count} game-platform relations for platform ID: {PlatformId}",
            gamePlatforms.Count(),
            platformId);
        return gamePlatforms;
    }

    private async Task<IEnumerable<GameGenre>> GetGameGenreRelationsAsync(Guid genreId)
    {
        var gameGenres = await _unitOfWork.GameGenres.GetByGenreIdAsync(genreId);
        _logger.LogInformation(
            "Found {Count} game-genre relations for genre ID: {GenreId}",
            gameGenres.Count(),
            genreId);
        return gameGenres;
    }

    private async Task<IEnumerable<GameCreateRequestDto>> GetGamesByRelations(List<Guid> gameIds)
    {
        if (gameIds.Count == 0)
        {
            return Enumerable.Empty<GameCreateRequestDto>();
        }

        var games = await _unitOfWork.Games.GetByIdsAsync(gameIds);
        _logger.LogInformation("Retrieved {Count} games", games.Count());
        return games.Select(MapToGameDto);
    }

    private static void UpdateGameFromDto(Game game, GameMetadataUpdateRequestDto gameRequest)
    {
        game.Name = gameRequest.Game.Name;
        game.Description = gameRequest.Game.Description ?? string.Empty;
        game.Price = gameRequest.Game.Price;
        game.UnitInStock = gameRequest.Game.UnitInStock;
        game.Discontinued = gameRequest.Game.Discontinued;
        game.PublisherId = gameRequest.Publisher;
    }

    private static Game CreateGameEntity(GameMetadataCreateRequestDto gameRequest)
    {
        return new Game
        {
            Name = gameRequest.Game.Name ?? throw new NullReferenceException("Name is null"),
            Key = gameRequest.Game.Key ?? throw new NullReferenceException("Key is null"),
            Description = gameRequest.Game.Description ?? throw new NullReferenceException("Description is null"),
            Price = gameRequest.Game.Price,
            UnitInStock = gameRequest.Game.UnitInStock,
            Discontinued = gameRequest.Game.Discount,
            PublisherId = gameRequest.Publisher,
        };
    }

    private static GameCreateRequestDto MapToGameDto(Game game)
    {
        return new GameCreateRequestDto
        {
            Name = game.Name,
            Key = game.Key,
            Description = game.Description,
            Price = game.Price,
            UnitInStock = game.UnitInStock,
            Discount = game.Discontinued,
        };
    }

    private static GameUpdateRequestDto MapToGameDtoUpdate(Game game)
    {
        return new GameUpdateRequestDto
        {
            Id = game.Id,
            Key = game.Key,
            Name = game.Name,
            Description = game.Description,
            Price = game.Price,
            UnitInStock = game.UnitInStock,
            Discontinued = game.Discontinued,
        };
    }

    private static string GenerateGameFilePath(Game game)
    {
        var sanitizedFileName = SanitizeFileName(game.Name);
        return Path.Combine(Environment.CurrentDirectory, $"{sanitizedFileName}.json");
    }

    private static async Task SaveGameFile(string filePath, string content)
    {
        await File.WriteAllTextAsync(filePath, content);
    }

    private static void ValidateObject(object obj, string paramName)
    {
        if (obj == null)
        {
            throw new ArgumentNullException(paramName);
        }
    }

    private static void ValidateString(string str, string paramName)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
        }
    }

    private static void ValidateGuid(Guid guid, string paramName)
    {
        if (guid == Guid.Empty)
        {
            throw new ArgumentException($"{paramName} cannot be empty", paramName);
        }
    }

    private static string GenerateKeyFromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or whitespace");
        }

        var key = Regex.Replace(name.ToLower(), @"[^a-z0-9]+", "-").Trim('-');
        return Regex.Replace(key, @"-+", "-");
    }
}