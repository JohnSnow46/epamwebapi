using Gamestore.Entities.Business;
using Gamestore.Services.Dto.FiltersDto;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services.Filters;

/// <summary>
/// Handles genre filtering in the game query pipeline.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GenreFilterHandler"/> class.
/// </remarks>
/// <param name="logger">The logger.</param>
public class GenreFilterHandler(ILogger<GenreFilterHandler> logger) : GameQueryHandlerBase
{
    private readonly ILogger<GenreFilterHandler> _logger = logger;

    /// <summary>
    /// Filters games by genre IDs.
    /// </summary>
    /// <param name="games">The list of games to filter.</param>
    /// <param name="parameters">The filter parameters.</param>
    /// <returns>The filtered game filter result.</returns>
    public override async Task<GameFilterResult> HandleAsync(IEnumerable<Game> games, GameFilterParameters parameters)
    {
        _logger.LogInformation("Filtering games by genres");

        var filteredGames = games;

        if (parameters.GenreIds != null && parameters.GenreIds.Count != 0)
        {
            _logger.LogInformation("Applying genre filter with {Count} genre IDs", parameters.GenreIds.Count);

            filteredGames = games.Where(g =>
                g.GameGenres != null &&
                g.GameGenres.Any(gg => parameters.GenreIds.Contains(gg.GenreId)));
        }

        return await PassToNextAsync(filteredGames, parameters);
    }
}