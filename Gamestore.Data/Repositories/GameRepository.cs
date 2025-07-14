using Gamestore.Data.Data;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Business;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repositories;

/// <summary>
/// Repository implementation for managing Game entities in the game catalog system.
/// Provides concrete implementations for game-specific operations including key-based lookups,
/// relationship queries, view count tracking, and comprehensive game data retrieval with related entities.
/// Inherits from the generic Repository pattern and implements IGameRepository interface.
/// </summary>
public class GameRepository(GameCatalogDbContext context) : Repository<Game>(context), IGameRepository
{
    private readonly GameCatalogDbContext _context = context;

    /// <summary>
    /// Retrieves a game by its unique key identifier with all related entities eagerly loaded.
    /// This method includes genres, platforms, and publisher information in a single query
    /// to provide complete game details for display and management operations.
    /// </summary>
    /// <param name="key">The unique string key of the game to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the Game entity
    /// with all related data if found, or null if no game with the specified key exists.
    /// </returns>
    public async Task<Game?> GetKeyAsync(string key)
    {
        return await _context.Games
            .Include(g => g.GameGenres)
                .ThenInclude(gg => gg.Genre)
            .Include(g => g.GamePlatforms)
                .ThenInclude(gp => gp.Platform)
            .Include(g => g.Publisher)
            .FirstOrDefaultAsync(g => g.Key == key);
    }

    /// <summary>
    /// Deletes a game entity from the database and immediately persists the changes.
    /// This method provides game-specific deletion logic with immediate database commitment,
    /// returning the deleted game entity for confirmation or further processing.
    /// </summary>
    /// <param name="game">The Game entity to delete from the database.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the deleted Game entity.
    /// </returns>
    public async Task<Game> DeleteGameByKey(Game game)
    {
        _context.Games.Remove(game);
        await _context.SaveChangesAsync();
        return game;
    }

    /// <summary>
    /// Retrieves all games that are available on a specific gaming platform.
    /// This method queries through the GamePlatform relationship table to find games
    /// compatible with the specified platform, useful for platform-specific browsing.
    /// </summary>
    /// <param name="platformId">The unique identifier of the platform to filter games by.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of Game entities
    /// available on the specified platform.
    /// </returns>
    public async Task<IEnumerable<Game>> GetByPlatformAsync(Guid platformId)
    {
        return await _context.GamePlatforms
                             .Where(gp => gp.PlatformId == platformId)
                             .Select(gp => gp.Game)
                             .ToListAsync();
    }

    /// <summary>
    /// Retrieves all games that belong to a specific genre.
    /// This method queries through the GameGenre relationship table to find games
    /// categorized under the specified genre, useful for genre-based browsing and filtering.
    /// </summary>
    /// <param name="genreId">The unique identifier of the genre to filter games by.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of Game entities
    /// belonging to the specified genre.
    /// </returns>
    public async Task<IEnumerable<Game>> GetByGenreAsync(Guid genreId)
    {
        return await _context.GameGenres
                             .Where(gg => gg.GenreId == genreId)
                             .Select(gg => gg.Game)
                             .ToListAsync();
    }

    /// <summary>
    /// Retrieves multiple games by their unique identifiers in a single optimized query.
    /// This method performs bulk retrieval of games, avoiding multiple individual database calls
    /// when working with collections of game IDs.
    /// </summary>
    /// <param name="gameIds">A list of unique identifiers for the games to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of Game entities
    /// that match the provided IDs. Games that don't exist are not included in the result.
    /// </returns>
    public async Task<IEnumerable<Game>> GetByIdsAsync(List<Guid> gameIds)
    {
        return await _context.Games
            .Where(g => gameIds.Contains(g.Id))
            .ToListAsync();
    }

    /// <summary>
    /// Gets the total number of games in the database using an optimized count operation.
    /// This method provides efficient counting without loading game entities into memory,
    /// useful for pagination, analytics, and reporting operations.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the total count of games.
    /// </returns>
    public async Task<int> CountAsync()
    {
        return await _context.Games.CountAsync();
    }

    /// <summary>
    /// Increments the view count for a game identified by its unique key.
    /// This method efficiently updates only the ViewCount property without loading
    /// the entire game entity, providing optimized tracking for game popularity analytics.
    /// </summary>
    /// <param name="key">The unique string key of the game to increment the view count for.</param>
    /// <returns>A task representing the asynchronous increment operation.</returns>
    public async Task IncrementViewCountAsync(string key)
    {
        var game = await _context.Games.FirstOrDefaultAsync(g => g.Key == key);
        if (game != null)
        {
            game.ViewCount++;
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Retrieves all games from the database with all related entities eagerly loaded.
    /// This method overrides the base implementation to include comprehensive game data
    /// including genres, platforms, and publisher information for complete game catalog display.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of all Game entities
    /// with their complete related data loaded.
    /// </returns>
    public override async Task<IEnumerable<Game>> GetAllAsync()
    {
        var games = await _context.Games
            .Include(g => g.GameGenres)
                .ThenInclude(gg => gg.Genre)
            .Include(g => g.GamePlatforms)
                .ThenInclude(gp => gp.Platform)
            .Include(g => g.Publisher)
            .ToListAsync();
        return games;
    }
}