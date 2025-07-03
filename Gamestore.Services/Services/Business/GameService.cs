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
/// </summary>
public partial class GameService(IUnitOfWork unitOfWork, ILogger<GameService> logger) : IGameService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<GameService> _logger = logger;


    #region Public Game methods

    /// <summary>
    /// Adds a new game with associated genres and platforms.
    /// </summary>
    public async Task<GameMetadataCreateRequestDto> AddGameAsync(GameMetadataCreateRequestDto gameRequest)
    {
        _logger.LogInformation("Starting add game operation for game: {GameName}", gameRequest.Game?.Name);

        ValidateObject(gameRequest, "Game request");
        ValidateObject(gameRequest.Game, "Game data");

        if (string.IsNullOrWhiteSpace(gameRequest.Game.Key))
        {
            ValidateString(gameRequest.Game.Name, "Game name");
            gameRequest.Game.Key = GenerateKeyFromName(gameRequest.Game.Name);
            _logger.LogInformation("Key was not provided, generated key: {Key} from name: {Name}", gameRequest.Game.Key, gameRequest.Game.Name);
        }

        await ValidateGameKeyUniqueness(gameRequest.Game.Key!);

        var newGame = CreateGameEntity(gameRequest);
        await _unitOfWork.Games.AddAsync(newGame);
        _logger.LogInformation("New game created with ID: {GameId}", newGame.Id);

        await AddGamePlatforms(newGame.Id, gameRequest.Platforms);
        await AddGameGenres(newGame.Id, gameRequest.Genres);

        await _unitOfWork.CompleteAsync();
        return gameRequest;
    }

    /// <summary>
    /// Updates an existing game by its key.
    /// </summary>
    public async Task<GameUpdateRequestDto> UpdateGameAsync(string key, GameMetadataUpdateRequestDto gameRequest)
    {
        _logger.LogInformation("Starting update game operation for key: {GameKey}", key);

        ValidateString(key, "Game key");
        var existingGame = await GetGameByKeyOrNull(key);
        if (existingGame == null)
        {
            return null;
        }

        UpdateGameFromDto(existingGame, gameRequest);
        await UpdateGameRelations(existingGame.Id, gameRequest);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Game updated successfully with ID: {GameId}", existingGame.Id);
        return gameRequest.Game;
    }

    /// <summary>
    /// Gets a game by its key.
    /// </summary>
    public async Task<GameUpdateRequestDto> GetGameByKey(string key)
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
    /// </summary>
    public async Task<Game> DeleteGameAsync(string key)
    {
        _logger.LogInformation("Starting deletion process for game with key: {Key}", key);

        ValidateString(key, "Game key");

        var game = await GetGameByKeyOrNull(key);

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
        return await GetGamesByRelations(gamePlatforms.Select(gp => gp.GameId).ToList());
    }

    /// <summary>
    /// Gets games by genre ID.
    /// </summary>
    public async Task<IEnumerable<GameCreateRequestDto>> GetGamesByGenreAsync(Guid genreId)
    {
        _logger.LogInformation("Starting get games by genre operation for genre ID: {GenreId}", genreId);

        ValidateGuid(genreId, "Genre ID");
        var gameGenres = await GetGameGenreRelationsAsync(genreId);
        return await GetGamesByRelations(gameGenres.Select(gg => gg.GameId).ToList());
    }

    /// <summary>
    /// Gets the total number of games.
    /// </summary>
    public async Task<int> GetTotalGamesCountAsync()
    {
        _logger.LogInformation("Starting get total games count operation");

        var count = await _unitOfWork.Games.CountAsync();
        _logger.LogInformation("Total games count: {GameCount}", count);
        return count;
    }

    #endregion

    #region Private Helper methods

    private void ValidateObject(object? obj, string paramName)
    {
        if (obj == null)
        {
            _logger.LogWarning("{ParamName} is null", paramName);
            throw new ArgumentNullException(paramName.ToLower(System.Globalization.CultureInfo.CurrentCulture).Replace(" ", string.Empty), $"{paramName} cannot be null");
        }
    }

    private void ValidateString(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            _logger.LogWarning("Provided {ParamName} is null or empty", paramName);
            throw new ArgumentException($"{paramName} cannot be null or empty", paramName.ToLower(System.Globalization.CultureInfo.CurrentCulture).Replace(" ", string.Empty));
        }
    }

    private void ValidateGuid(Guid id, string paramName)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Provided {ParamName} is empty", paramName);
            throw new ArgumentException($"{paramName} cannot be empty", paramName.ToLower(System.Globalization.CultureInfo.CurrentCulture).Replace(" ", string.Empty));
        }
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
            throw new KeyNotFoundException($"Game with ID '{id}' not found");
        }
    }

    private async Task<Game> GetGameByKeyOrNull(string key)
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
        _logger.LogInformation("Found {Count} game-platform relations for platform ID: {PlatformId}", gamePlatforms.Count(), platformId);
        return gamePlatforms;
    }

    private async Task<IEnumerable<GameGenre>> GetGameGenreRelationsAsync(Guid genreId)
    {
        var gameGenres = await _unitOfWork.GameGenres.GetByGenreIdAsync(genreId);
        _logger.LogInformation("Found {Count} game-genre relations for genre ID: {GenreId}", gameGenres.Count(), genreId);
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
            _logger.LogInformation("Removed {Count} existing genres from game: {GameId}", existingGenres.Count, gameId);
        }

        await AddGameGenres(gameId, genreIds);
    }

    private async Task UpdateGamePlatforms(Guid gameId, List<Guid>? platformIds)
    {
        var existingPlatforms = await _unitOfWork.GamePlatforms.GetByGameIdAsync(gameId);
        if (existingPlatforms.Count != 0)
        {
            await _unitOfWork.GamePlatforms.RemoveRangeAsync(existingPlatforms);
            _logger.LogInformation("Removed {Count} existing platforms from game: {GameId}", existingPlatforms.Count, gameId);
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
            _logger.LogInformation("Removing {Count} genre associations for game ID: {GameId}", game.GameGenres.Count, game.Id);
            await _unitOfWork.GameGenres.RemoveRangeAsync(game.GameGenres);
        }

        if (game.GamePlatforms?.Count > 0)
        {
            _logger.LogInformation("Removing {Count} platform associations for game ID: {GameId}", game.GamePlatforms.Count, game.Id);
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

    private static string GenerateGameFilePath(Game game)
    {
        var sanitizedName = SanitizeFileName(game.Name);
        var uniqueFileName = $"{sanitizedName}_{game.Id}.txt";
        return Path.Combine(Directory.GetCurrentDirectory(), "GameFiles", uniqueFileName);
    }

    private async Task SaveGameFile(string filePath, string content)
    {
        _logger.LogInformation("Preparing to save file at path: {FilePath}", filePath);

        var directoryPath = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            _logger.LogInformation("Created directory for game files at: {DirectoryPath}", directoryPath);
        }

        await File.WriteAllTextAsync(filePath, content);
    }

    private static Game CreateGameEntity(GameMetadataCreateRequestDto gameRequest)
    {
        return new Game
        {
            Id = Guid.NewGuid(),
            Name = gameRequest.Game.Name,
            Key = gameRequest.Game.Key,
            Description = gameRequest.Game.Description ?? string.Empty,
            Price = gameRequest.Game.Price,
            UnitInStock = gameRequest.Game.UnitInStock,
            Discontinued = gameRequest.Game.Discount,
            PublisherId = gameRequest.Publisher != Guid.Empty ? gameRequest.Publisher : null,
        };
    }

    private static void UpdateGameFromDto(Game game, GameMetadataUpdateRequestDto gameRequest)
    {
        game.Name = gameRequest.Game.Name;
        game.Description = gameRequest.Game.Description ?? string.Empty;
        game.Price = gameRequest.Game.Price;
        game.UnitInStock = gameRequest.Game.UnitInStock;
        game.Discontinued = gameRequest.Game.Discontinued;
        game.PublisherId = gameRequest.Publisher != Guid.Empty ? gameRequest.Publisher : null;
    }

    private static GameCreateRequestDto MapToGameDto(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);

        return new GameCreateRequestDto
        {
            Description = game.Description,
            Key = game.Key,
            Name = game.Name,
            Price = game.Price,
            UnitInStock = game.UnitInStock,
            Discount = game.Discontinued,
        };
    }

    private static GameUpdateRequestDto MapToGameDtoUpdate(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);

        return new GameUpdateRequestDto
        {
            Id = game.Id,
            Name = game.Name,
            Key = game.Key,
            Description = game.Description,
            Price = game.Price,
            UnitInStock = game.UnitInStock,
            Discontinued = game.Discontinued,
        };
    }

    /// <summary>
    /// Generates a URL-friendly key from a game name.
    /// </summary>
    private static string GenerateKeyFromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Game name cannot be empty when generating a key", nameof(name));
        }

        // Convert to lowercase
        var key = name.ToLowerInvariant();

        // Replace spaces with hyphens
        key = key.Replace(" ", "-");

        // Remove any special characters and keep only alphanumeric and hyphens
        key = Regex.Replace(key, "[^a-z0-9-]", string.Empty);

        // Replace multiple hyphens with a single hyphen
        key = Regex.Replace(key, "-+", "-");

        // Trim hyphens from start and end
        key = key.Trim('-');

        return key;
    }

    #endregion
}