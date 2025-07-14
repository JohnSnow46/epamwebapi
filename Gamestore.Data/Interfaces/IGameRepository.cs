using Gamestore.Entities.Business;

namespace Gamestore.Data.Interfaces;

/// <summary>
/// Repository interface for managing Game entities in the system.
/// Provides specialized operations for game-specific queries, including filtering by platform/genre,
/// key-based operations, view count tracking, and bulk operations.
/// Extends the generic repository pattern with game-specific business logic.
/// </summary>
public interface IGameRepository : IRepository<Game>
{
    /// <summary>
    /// Retrieves a game by its unique key identifier.
    /// The key is a string-based unique identifier used for URL-friendly game references.
    /// </summary>
    /// <param name="key">The unique string key of the game to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the Game entity
    /// if found, or null if no game with the specified key exists.
    /// </returns>
    Task<Game?> GetKeyAsync(string key);

    /// <summary>
    /// Deletes a game entity from the database.
    /// This method provides game-specific delete logic that may include additional business rules
    /// compared to the generic Delete operation.
    /// </summary>
    /// <param name="game">The Game entity to delete from the database.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the deleted Game entity.
    /// </returns>
    Task<Game> DeleteGameByKey(Game game);

    /// <summary>
    /// Retrieves all games that are available on a specific gaming platform.
    /// This method queries games through their GamePlatform relationships.
    /// </summary>
    /// <param name="platformId">The unique identifier of the platform to filter games by.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of Game entities
    /// that are available on the specified platform.
    /// </returns>
    Task<IEnumerable<Game>> GetByPlatformAsync(Guid platformId);

    /// <summary>
    /// Retrieves all games that belong to a specific genre.
    /// This method queries games through their GameGenre relationships.
    /// </summary>
    /// <param name="genreId">The unique identifier of the genre to filter games by.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of Game entities
    /// that belong to the specified genre.
    /// </returns>
    Task<IEnumerable<Game>> GetByGenreAsync(Guid genreId);

    /// <summary>
    /// Retrieves multiple games by their unique identifiers in a single operation.
    /// This method is optimized for bulk retrieval scenarios.
    /// </summary>
    /// <param name="gameIds">A list of unique identifiers for the games to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of Game entities
    /// that match the provided IDs. Games that don't exist are not included in the result.
    /// </returns>
    Task<IEnumerable<Game>> GetByIdsAsync(List<Guid> gameIds);

    /// <summary>
    /// Gets the total number of games in the database.
    /// This method provides an efficient way to count all games without loading them into memory.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the total count of games.
    /// </returns>
    Task<int> CountAsync();

    /// <summary>
    /// Retrieves all games from the database.
    /// This method overrides the base repository implementation to provide game-specific logic
    /// such as including related entities or applying default sorting.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of all Game entities.
    /// </returns>
    new Task<IEnumerable<Game>> GetAllAsync();

    /// <summary>
    /// Increments the view count for a game identified by its unique key.
    /// This method is used for tracking game popularity and analytics.
    /// The operation is optimized to avoid loading the entire game entity.
    /// </summary>
    /// <param name="key">The unique string key of the game to increment the view count for.</param>
    /// <returns>A task representing the asynchronous increment operation.</returns>
    Task IncrementViewCountAsync(string key);
}