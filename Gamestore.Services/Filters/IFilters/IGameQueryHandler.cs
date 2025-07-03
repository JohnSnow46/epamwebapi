using Gamestore.Entities.Business;
using Gamestore.Services.Dto.FiltersDto;

namespace Gamestore.Services.Filters.IFilters;

/// <summary>
/// Interface for a game query pipeline handler.
/// </summary>
public interface IGameQueryHandler
{
    /// <summary>
    /// Sets the next handler in the pipeline.
    /// </summary>
    /// <param name="handler">The next handler.</param>
    /// <returns>The next handler.</returns>
    IGameQueryHandler SetNext(IGameQueryHandler handler);

    /// <summary>
    /// Handles the current processing step and passes to the next handler.
    /// </summary>
    /// <param name="games">The list of games to process.</param>
    /// <param name="parameters">The filter parameters.</param>
    /// <returns>The processed game filter result.</returns>
    Task<GameFilterResult> HandleAsync(IEnumerable<Game> games, GameFilterParameters parameters);
}
