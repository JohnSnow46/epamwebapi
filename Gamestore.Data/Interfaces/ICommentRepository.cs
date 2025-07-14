using Gamestore.Entities.Community;

namespace Gamestore.Data.Interfaces;

/// <summary>
/// Repository interface for managing Comment entities in the community system.
/// Provides specialized operations for comment hierarchies, game-specific comment retrieval,
/// and managing parent-child comment relationships.
/// Extends the generic repository pattern with comment-specific business logic.
/// </summary>
public interface ICommentRepository : IRepository<Comment>
{
    /// <summary>
    /// Retrieves all comments associated with a specific game using the game's unique key.
    /// This method includes both root comments and their nested child comments in a hierarchical structure.
    /// </summary>
    /// <param name="gameKey">The unique string key of the game to retrieve comments for.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of Comment entities
    /// associated with the specified game. If the game doesn't exist or has no comments, an empty collection is returned.
    /// </returns>
    Task<IEnumerable<Comment>> GetCommentsByGameKeyAsync(string gameKey);

    /// <summary>
    /// Retrieves a specific comment by its unique identifier, including its child comments.
    /// This method loads the complete comment hierarchy starting from the specified comment.
    /// </summary>
    /// <param name="id">The unique identifier of the comment to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the Comment entity
    /// if found, or null if no comment with the specified ID exists. Includes child comments when found.
    /// </returns>
    Task<Comment?> GetCommentByIdAsync(Guid id);

    /// <summary>
    /// Retrieves only the root-level comments for a specific game (comments without parent comments).
    /// This method is useful for displaying the main discussion threads without nested replies.
    /// Each root comment includes its complete hierarchy of child comments.
    /// </summary>
    /// <param name="gameId">The unique identifier of the game to retrieve root comments for.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of root Comment entities
    /// for the specified game. Each comment includes its nested child comments for complete thread display.
    /// </returns>
    Task<IEnumerable<Comment>> GetRootCommentsByGameIdAsync(Guid gameId);
}