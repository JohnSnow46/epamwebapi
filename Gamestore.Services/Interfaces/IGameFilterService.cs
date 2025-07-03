using Gamestore.Services.Dto.FiltersDto;

namespace Gamestore.Services.Interfaces;
/// <summary>
/// Interface for the game filter service.
/// </summary>
public interface IGameFilterService
{
    /// <summary>
    /// Gets the pagination options.
    /// </summary>
    /// <returns>The list of pagination options.</returns>
    List<string> GetPaginationOptions();

    /// <summary>
    /// Gets the sorting options.
    /// </summary>
    /// <returns>The list of sorting options.</returns>
    List<string> GetSortingOptions();

    /// <summary>
    /// Gets the publishing date filter options.
    /// </summary>
    /// <returns>The list of publishing date filter options.</returns>
    List<string> GetPublishDateFilterOptions();

    /// <summary>
    /// Gets filtered games based on the specified parameters.
    /// </summary>
    /// <param name="parameters">The filter parameters.</param>
    /// <returns>The filtered game result.</returns>
    Task<GameFilterResult> GetFilteredGamesAsync(GameFilterParameters parameters);

    /// <summary>
    /// Increments the view count for a game by its key.
    /// </summary>
    /// <param name="gameKey">The game key.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task IncrementGameViewCountAsync(string gameKey);
}
