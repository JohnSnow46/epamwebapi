using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Business;
using Gamestore.Services.Dto.GamesDto;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Gamestore.Services.Services.Business;

/// <summary>
/// Service for managing games in the Gamestore.
/// Epic 10: Added Azure Blob Storage integration for game images.
/// </summary>
public partial class GameService(
    IUnitOfWork unitOfWork,
    ILogger<GameService> logger,
    IBlobStorageService blobStorageService) : IGameService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<GameService> _logger = logger;
    private readonly IBlobStorageService _blobStorageService = blobStorageService;

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

        // Epic 10 US1: Upload image if provided
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
        return gameRequest;
    }

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

        // Epic 10 US2: Handle image update
        if (!string.IsNullOrEmpty(gameRequest.Image))
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(existingGame.ImageUrl))
                {
                    var oldBlobName = _blobStorageService.GetBlobNameFromGameKey(key);
                    await _blobStorageService.DeleteImageAsync(oldBlobName);
                    _logger.LogInformation("Deleted old image for game: {GameKey}", key);
                }

                var newImageUrl = await _blobStorageService.UploadImageAsync(
                    gameRequest.Image,
                    key);
                existingGame.ImageUrl = newImageUrl;
                _logger.LogInformation("Updated image for game: {GameKey}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update image for game: {GameKey}. Game will be updated without new image.", key);
            }
        }

        await _unitOfWork.CompleteAsync();
        _logger.LogInformation("Game updated successfully with ID: {GameId}", existingGame.Id);

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
        var existingGame = await GetGameByKeyOrNull(key);

        if (existingGame == null)
        {
            return null;
        }

        var gameDetails = MapToGameDtoUpdate(existingGame);
        _logger.LogInformation("Successfully retrieved game with ID: {GameId}", existingGame.Id);

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

        // Epic 10 US3: Delete associated image from blob storage
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

        _logger.LogInformation("Successfully deleted game with key: {Key}, ID: {Id}", key, game.Id);
        return game;
    }

    /// <summary>
    /// Gets all games.
    /// </summary>
    public async Task<IEnumerable<GameCreateRequestDto>> GetAllGames()
    {
        _logger.LogInformation("Starting get all games operation");

        var games = await _unitOfWork.Games.GetAllAsync();
        var gameList = games.ToList();

        _logger.LogInformation("Retrieved {Count} games from database", gameList.Count);
        return gameList.Select(MapToGameDto);
    }

    /// <summary>
    /// Creates a file with serialized game data using the game key.
    /// </summary>
    public async Task<string> CreateGameFileAsync(string gameKey)
    {
        _logger.LogInformation("Starting create game file operation for game key: {GameKey}", gameKey);

        ValidateString(gameKey, "Game key");
        var game = await GetGameByKeyOrNull(gameKey) ?? throw new KeyNotFoundException($"Game with key '{gameKey}' not found");

        var serializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented,
        };

        var serializedGame = JsonConvert.SerializeObject(game, serializerSettings);
        var filePath = GenerateGameFilePath(game);

        await SaveGameFile(filePath, serializedGame);

        _logger.LogInformation("Game file for game with key: {GameKey} successfully written", gameKey);
        return filePath;
    }

    /// <summary>
    /// Gets games by platform ID.
    /// </summary>
    public async Task<IEnumerable<GameCreateRequestDto>> GetGamesByPlatformAsync(Guid platformId)
    {
        _logger.LogInformation("Starting get games by platform operation for platform ID: {PlatformId}", platformId);

        ValidateGuid(platformId, "Platform ID");

        var gamePlatforms = await GetGamePlatformRelationsAsync(platformId);
        var gameIds = gamePlatforms.Select(gp => gp.GameId).ToList();

        return await GetGamesByRelations(gameIds);
    }

    /// <summary>
    /// Gets games by genre ID.
    /// </summary>
    public async Task<IEnumerable<GameCreateRequestDto>> GetGamesByGenreAsync(Guid genreId)
    {
        _logger.LogInformation("Starting get games by genre operation for genre ID: {GenreId}", genreId);

        ValidateGuid(genreId, "Genre ID");

        var gameGenres = await GetGameGenreRelationsAsync(genreId);
        var gameIds = gameGenres.Select(gg => gg.GameId).ToList();

        return await GetGamesByRelations(gameIds);
    }

    /// <summary>
    /// Gets the total number of games.
    /// </summary>
    public async Task<int> GetTotalGamesCountAsync()
    {
        _logger.LogInformation("Getting total games count");
        var count = await _unitOfWork.Games.CountAsync();
        _logger.LogInformation("Total games count: {Count}", count);
        return count;
    }

    // Private helper methods below...
    private static void ValidateObject(object? obj, string paramName)
    {
        if (obj == null)
        {
            throw new ArgumentNullException(paramName, $"{paramName} cannot be null");
        }
    }

    private static void ValidateString(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
        }
    }

    private static void ValidateGuid(Guid value, string paramName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException($"{paramName} cannot be empty", paramName);
        }
    }

    [GeneratedRegex(@"[^a-zA-Z0-9\s-]")]
    private static partial Regex InvalidCharactersRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    private static string GenerateKeyFromName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or empty", nameof(name));
        }

        var normalizedName = name.ToLowerInvariant();
        normalizedName = InvalidCharactersRegex().Replace(normalizedName, string.Empty);
        normalizedName = WhitespaceRegex().Replace(normalizedName, "-");

        return normalizedName.Trim('-');
    }

    private static Game CreateGameEntity(GameMetadataCreateRequestDto gameRequest)
    {
        return new Game
        {
            Key = gameRequest.Game.Key!,
            Name = gameRequest.Game.Name,
            Description = gameRequest.Game.Description ?? throw new NullReferenceException("Description is null"),
            Price = gameRequest.Game.Price,
            UnitInStock = gameRequest.Game.UnitInStock,
            Discontinued = gameRequest.Game.Discount,
            PublisherId = gameRequest.Publisher,
        };
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
}