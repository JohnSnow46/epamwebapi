using Gamestore.Entities.Business;

namespace Gamestore.Data.Interfaces;

/// <summary>
/// Repository interface for managing the many-to-many relationships between games and gaming platforms.
/// Provides specialized operations for handling GamePlatform associations, bulk operations,
/// and querying relationships by game or platform identifiers.
/// </summary>
public interface IGamePlatformRepository
{
    /// <summary>
    /// Retrieves a list of gaming platforms by their unique identifiers.
    /// This method is typically used to fetch platform details when you have a collection of platform IDs.
    /// </summary>
    /// <param name="ids">A list of unique identifiers for the platforms to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a list of Platform entities
    /// that match the provided IDs. If no platforms are found, an empty list is returned.
    /// </returns>
    Task<List<Platform>> GetByIdsAsync(List<Guid> ids);

    /// <summary>
    /// Retrieves all GamePlatform relationships for a specific game.
    /// This method is useful for finding all gaming platforms that a particular game supports.
    /// </summary>
    /// <param name="gameId">The unique identifier of the game to find platform relationships for.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a list of GamePlatform entities
    /// that represent the relationship between the specified game and its supported platforms.
    /// </returns>
    Task<List<GamePlatform>> GetByGameIdAsync(Guid gameId);

    /// <summary>
    /// Removes multiple GamePlatform relationships from the database in a single operation.
    /// This method performs a bulk delete operation and saves changes to the database immediately.
    /// </summary>
    /// <param name="gamePlatforms">The collection of GamePlatform entities to remove from the database.</param>
    /// <returns>A task representing the asynchronous remove operation.</returns>
    Task RemoveRangeAsync(IEnumerable<GamePlatform> gamePlatforms);

    /// <summary>
    /// Adds multiple GamePlatform relationships to the database in a single operation.
    /// This method performs a bulk insert operation and saves changes to the database immediately.
    /// </summary>
    /// <param name="gamePlatforms">The collection of GamePlatform entities to add to the database.</param>
    /// <returns>A task representing the asynchronous add operation.</returns>
    Task AddRangeAsync(IEnumerable<GamePlatform> gamePlatforms);

    /// <summary>
    /// Retrieves all GamePlatform relationships for a specific gaming platform.
    /// This method is useful for finding all games that are available on a particular platform.
    /// </summary>
    /// <param name="platformId">The unique identifier of the platform to find game relationships for.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of GamePlatform entities
    /// that represent the relationship between the specified platform and its available games.
    /// </returns>
    Task<IEnumerable<GamePlatform>> GetByPlatformIdAsync(Guid platformId);
}