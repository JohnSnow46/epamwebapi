using Gamestore.Entities.Business;
using Gamestore.Services.Dto.FiltersDto;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services.Filters;

/// <summary>
/// Handles sorting in the game query pipeline.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SortingHandler"/> class.
/// </remarks>
/// <param name="logger">The logger.</param>
public class SortingHandler(ILogger<SortingHandler> logger) : GameQueryHandlerBase
{
    private readonly ILogger<SortingHandler> _logger = logger;

    /// <summary>
    /// Sorts games based on the specified criteria.
    /// </summary>
    /// <param name="games">The list of games to sort.</param>
    /// <param name="parameters">The filter parameters.</param>
    /// <returns>The sorted game filter result.</returns>
    public override async Task<GameFilterResult> HandleAsync(IEnumerable<Game> games, GameFilterParameters parameters)
    {
        _logger.LogInformation("Sorting games by {SortBy}", parameters.SortBy);

        var sortedGames = parameters.SortBy switch
        {
            "Most popular" => games.OrderByDescending(g => g.ViewCount),
            "Most commented" => games.OrderByDescending(g => g.CommentCount),
            "Price ASC" => games.OrderBy(g => g.Price),
            "Price DESC" => games.OrderByDescending(g => g.Price),
            "New" => games.OrderByDescending(g => g.Id),
            _ => games.OrderByDescending(g => g.Id)
        };

        return await PassToNextAsync(sortedGames, parameters);
    }
}
