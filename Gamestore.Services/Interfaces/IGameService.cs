using Gamestore.Entities.Business;
using Gamestore.Services.Dto.GamesDto;

namespace Gamestore.Services.Interfaces;

/// <summary>
/// Service interface for managing game operations including CRUD operations,
/// file management, and game filtering capabilities.
/// </summary>
public interface IGameService
{
    /// <summary>
    /// Creates a new game with the provided metadata.
    /// </summary>
    /// <param name="gameRequest">Game creation request containing metadata</param>
    /// <returns>Created game metadata</returns>
    Task<GameMetadataCreateRequestDto> AddGameAsync(GameMetadataCreateRequestDto gameRequest);

    /// <summary>
    /// Updates an existing game identified by key.
    /// </summary>
    /// <param name="key">Game's unique key</param>
    /// <param name="gameRequest">Updated game metadata</param>
    /// <returns>Updated game data</returns>
    Task<GameUpdateRequestDto> UpdateGameAsync(string key, GameMetadataUpdateRequestDto gameRequest);

    /// <summary>
    /// Retrieves a game by its unique key.
    /// </summary>
    /// <param name="key">Game's unique key</param>
    /// <returns>Game data or null if not found</returns>
    Task<GameUpdateRequestDto> GetGameByKey(string key);

    Task<GameCreateRequestDto> GetGameById(Guid id);

    Task<Game> DeleteGameAsync(string key);

    Task<IEnumerable<GameCreateRequestDto>> GetAllGames();

    /// <summary>
    /// Creates a downloadable file for the specified game.
    /// </summary>
    /// <param name="gameKey">Game's unique key</param>
    /// <returns>File path or identifier</returns>
    Task<string> CreateGameFileAsync(string gameKey);

    Task<IEnumerable<GameCreateRequestDto>> GetGamesByPlatformAsync(Guid platformId);

    Task<IEnumerable<GameCreateRequestDto>> GetGamesByGenreAsync(Guid genreId);

    /// <summary>
    /// Gets the total number of games in the system.
    /// </summary>
    /// <returns>Total games count</returns>
    Task<int> GetTotalGamesCountAsync();
}
