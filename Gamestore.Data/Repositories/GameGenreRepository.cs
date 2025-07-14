using Gamestore.Data.Data;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Business;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repositories;

/// <summary>
/// Repository implementation for managing GameGenre relationships in the game catalog system.
/// Provides concrete implementations for many-to-many relationship operations between games and genres,
/// including bulk operations, relationship queries, and efficient genre management.
/// Inherits from the generic Repository pattern and implements IGameGenreRepository interface.
/// </summary>
public class GameGenreRepository(GameCatalogDbContext context) : Repository<Genre>(context), IGameGenreRepository
{
    private readonly GameCatalogDbContext _context = context;

    /// <summary>
    /// Retrieves multiple genres by their unique identifiers in a single optimized query.
    /// This method performs a bulk lookup operation to fetch genre details when working
    /// with collections of genre IDs, avoiding multiple individual database calls.
    /// </summary>
    /// <param name="ids">A list of unique identifiers for the genres to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a list of Genre entities
    /// that match the provided IDs. Genres that don't exist are not included in the result.
    /// </returns>
    public async Task<List<Genre>> GetByIdsAsync(List<Guid> ids)
    {
        return await _context.Genres
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();
    }

    /// <summary>
    /// Removes multiple GameGenre relationships from the database in a single bulk operation.
    /// This method performs efficient bulk deletion of game-genre associations and immediately
    /// persists the changes to the database. Used for removing genre assignments from games.
    /// </summary>
    /// <param name="gameGenres">The collection of GameGenre entities to remove from the database.</param>
    /// <returns>A task representing the asynchronous bulk removal operation.</returns>
    public async Task RemoveRangeAsync(IEnumerable<GameGenre> gameGenres)
    {
        _context.GameGenres.RemoveRange(gameGenres);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Retrieves all GameGenre relationships for a specific game.
    /// This method finds all genre associations for a particular game, useful for displaying
    /// game categories, filtering operations, and game classification management.
    /// </summary>
    /// <param name="gameId">The unique identifier of the game to find genre relationships for.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a list of GameGenre entities
    /// representing all genre associations for the specified game.
    /// </returns>
    public async Task<List<GameGenre>> GetByGameIdAsync(Guid gameId)
    {
        return await _context.GameGenres
            .Where(g => g.GameId == gameId)
            .ToListAsync();
    }

    /// <summary>
    /// Adds multiple GameGenre relationships to the database in a single bulk operation.
    /// This method performs efficient bulk insertion of new game-genre associations and immediately
    /// persists the changes to the database. Used for assigning multiple genres to games.
    /// </summary>
    /// <param name="gameGenres">The collection of GameGenre entities to add to the database.</param>
    /// <returns>A task representing the asynchronous bulk addition operation.</returns>
    public async Task AddRangeAsync(IEnumerable<GameGenre> gameGenres)
    {
        await _context.GameGenres.AddRangeAsync(gameGenres);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Retrieves all GameGenre relationships for a specific genre.
    /// This method finds all game associations for a particular genre, useful for displaying
    /// games within a category, genre-based browsing, and content organization.
    /// </summary>
    /// <param name="genreId">The unique identifier of the genre to find game relationships for.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of GameGenre entities
    /// representing all game associations for the specified genre.
    /// </returns>
    public async Task<IEnumerable<GameGenre>> GetByGenreIdAsync(Guid genreId)
    {
        return await _context.GameGenres
            .Where(gg => gg.GenreId == genreId)
            .ToListAsync();
    }
}