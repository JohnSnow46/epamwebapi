using Gamestore.Data.Data;
using Gamestore.Data.Interfaces;
using Gamestore.Entities.Community;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repositories;

/// <summary>
/// Repository implementation for managing Comment entities in the community discussion system.
/// Provides concrete implementations for comment hierarchy operations, game-specific comment retrieval,
/// and parent-child comment relationship management with eager loading of nested comments.
/// Inherits from the generic Repository pattern and implements ICommentRepository interface.
/// </summary>
public class CommentRepository(GameCatalogDbContext context) : Repository<Comment>(context), ICommentRepository
{
    private readonly GameCatalogDbContext _context = context;

    /// <summary>
    /// Retrieves all comments associated with a specific game using the game's unique key.
    /// First locates the game by its key, then retrieves all root comments with their hierarchical structure.
    /// Returns an empty collection if the game doesn't exist or has no comments.
    /// </summary>
    /// <param name="gameKey">The unique string key of the game to retrieve comments for.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of Comment entities
    /// associated with the specified game, including their nested child comments.
    /// </returns>
    public async Task<IEnumerable<Comment>> GetCommentsByGameKeyAsync(string gameKey)
    {
        var game = await _context.Games
            .FirstOrDefaultAsync(g => g.Key == gameKey);

        return game == null ? Enumerable.Empty<Comment>() : await GetRootCommentsByGameIdAsync(game.Id);
    }

    /// <summary>
    /// Retrieves a specific comment by its unique identifier, including its child comments.
    /// Uses eager loading to include the complete comment hierarchy starting from the specified comment.
    /// This enables displaying comment threads with nested replies in a single query.
    /// </summary>
    /// <param name="id">The unique identifier of the comment to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the Comment entity
    /// with loaded child comments if found, or null if no comment with the specified ID exists.
    /// </returns>
    public async Task<Comment?> GetCommentByIdAsync(Guid id)
    {
        return await _context.Comments
            .Include(c => c.ChildComments)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    /// <summary>
    /// Retrieves only the root-level comments for a specific game (comments without parent comments).
    /// Filters comments to include only those with null ParentCommentId and eagerly loads their child comments
    /// to provide complete discussion threads. This method is optimized for displaying main discussion topics.
    /// </summary>
    /// <param name="gameId">The unique identifier of the game to retrieve root comments for.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of root Comment entities
    /// for the specified game, each including its complete hierarchy of nested child comments.
    /// </returns>
    public async Task<IEnumerable<Comment>> GetRootCommentsByGameIdAsync(Guid gameId)
    {
        return await _context.Comments
            .Where(c => c.GameId == gameId && c.ParentCommentId == null)
            .Include(c => c.ChildComments)
            .ToListAsync();
    }
}