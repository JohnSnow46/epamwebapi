using Gamestore.Data.Data;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Business;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repositories;

/// <summary>
/// Repository implementation for managing GamePlatform relationships in the game catalog system.
/// Provides concrete implementations for many-to-many relationship operations between games and gaming platforms,
/// including bulk operations, relationship queries, and efficient platform compatibility management.
/// Inherits from the generic Repository pattern and implements IGamePlatformRepository interface.
/// </summary>
public class GamePlatformRepository(GameCatalogDbContext context) : Repository<Platform>(context), IGamePlatformRepository
{
    private readonly GameCatalogDbContext _context = context;

    /// <summary>
    /// Retrieves multiple gaming platforms by their unique identifiers in a single optimized query.
    /// This method performs a bulk lookup operation to fetch platform details when working
    /// with collections of platform IDs, avoiding multiple individual database calls.
    /// </summary>
    /// <param name="ids">A list of unique identifiers for the platforms to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a list of Platform entities
    /// that match the provided IDs. Platforms that don't exist are not included in the result.
    /// </returns>
    public async Task<List<Platform>> GetByIdsAsync(List<Guid> ids)
    {
        return await _context.Platforms
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();
    }

    /// <summary>
    /// Removes multiple GamePlatform relationships from the database in a single bulk operation.
    /// This method performs efficient bulk deletion of game-platform associations and immediately
    /// persists the changes to the database. Used for removing platform compatibility from games.
    /// </summary>
    /// <param name="gamePlatforms">The collection of GamePlatform entities to remove from the database.</param>
    /// <returns>A task representing the asynchronous bulk removal operation.</returns>
    public async Task RemoveRangeAsync(IEnumerable<GamePlatform> gamePlatforms)
    {
        _context.GamePlatforms.RemoveRange(gamePlatforms);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Retrieves all GamePlatform relationships for a specific game.
    /// This method finds all platform associations for a particular game, useful for displaying
    /// supported platforms, compatibility information, and platform-specific game management.
    /// </summary>
    /// <param name="gameId">The unique identifier of the game to find platform relationships for.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a list of GamePlatform entities
    /// representing all platform associations for the specified game.
    /// </returns>
    public async Task<List<GamePlatform>> GetByGameIdAsync(Guid gameId)
    {
        return await _context.GamePlatforms
            .Where(gp => gp.GameId == gameId)
            .ToListAsync();
    }

    /// <summary>
    /// Adds multiple GamePlatform relationships to the database in a single bulk operation.
    /// This method performs efficient bulk insertion of new game-platform associations and immediately
    /// persists the changes to the database. Used for establishing platform compatibility for games.
    /// </summary>
    /// <param name="gamePlatforms">The collection of GamePlatform entities to add to the database.</param>
    /// <returns>A task representing the asynchronous bulk addition operation.</returns>
    public async Task AddRangeAsync(IEnumerable<GamePlatform> gamePlatforms)
    {
        await _context.GamePlatforms.AddRangeAsync(gamePlatforms);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Retrieves all GamePlatform relationships for a specific gaming platform.
    /// This method finds all game associations for a particular platform, useful for displaying
    /// platform-exclusive titles, platform-based game browsing, and inventory management.
    /// </summary>
    /// <param name="platformId">The unique identifier of the platform to find game relationships for.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of GamePlatform entities
    /// representing all game associations for the specified platform.
    /// </returns>
    public async Task<IEnumerable<GamePlatform>> GetByPlatformIdAsync(Guid platformId)
    {
        return await _context.GamePlatforms
            .Where(gp => gp.PlatformId == platformId)
            .ToListAsync();
    }
}