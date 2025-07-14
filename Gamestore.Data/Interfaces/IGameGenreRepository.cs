using Gamestore.Entities.Business;

namespace Gamestore.Data.Interfaces;

/// <summary>
/// Repository interface for managing the many-to-many relationships between games and genres.
/// Provides specialized operations for handling GameGenre associations, bulk operations,
/// and querying relationships by game or genre identifiers.
/// </summary>
public interface IGameGenreRepository
{
    /// <summary>
    /// Retrieves a list of genres by their unique identifiers.
    /// This method is typically used to fetch genre details when you have a collection of genre IDs.
    /// </summary>
    /// <param name="ids">A list of unique identifiers for the genres to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a list of Genre entities
    /// that match the provided IDs. If no genres are found, an empty list is returned.
    /// </returns>
    Task<List<Genre>> GetByIdsAsync(List<Guid> ids);

    /// <summary>
    /// Removes multiple GameGenre relationships from the database in a single operation.
    /// This method performs a bulk delete operation and saves changes to the database.
    /// </summary>
    /// <param name="gameGenres">The collection of GameGenre entities to remove from the database.</param>
    /// <returns>A task representing the asynchronous remove operation.</returns>
    Task RemoveRangeAsync(IEnumerable<GameGenre> gameGenres);

    /// <summary>
    /// Adds multiple GameGenre relationships to the database in a single operation.
    /// This method performs a bulk insert operation and saves changes to the database.
    /// </summary>
    /// <param name="gameGenres">The collection of GameGenre entities to add to the database.</param>
    /// <returns>A task representing the asynchronous add operation.</returns>
    Task AddRangeAsync(IEnumerable<GameGenre> gameGenres);

    /// <summary>
    /// Retrieves all GameGenre relationships for a specific game.
    /// This method is useful for finding all genres associated with a particular game.
    /// </summary>
    /// <param name="gameId">The unique identifier of the game to find genre relationships for.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a list of GameGenre entities
    /// that represent the relationship between the specified game and its associated genres.
    /// </returns>
    Task<List<GameGenre>> GetByGameIdAsync(Guid gameId);

    /// <summary>
    /// Retrieves all GameGenre relationships for a specific genre.
    /// This method is useful for finding all games that belong to a particular genre.
    /// </summary>
    /// <param name="genreId">The unique identifier of the genre to find game relationships for.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of GameGenre entities
    /// that represent the relationship between the specified genre and its associated games.
    /// </returns>
    Task<IEnumerable<GameGenre>> GetByGenreIdAsync(Guid genreId);
}