using Gamestore.Entities.Business;
using Gamestore.Services.Dto.FiltersDto;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services.Filters;
/// <summary>
/// Handles price range filtering in the game query pipeline.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PriceFilterHandler"/> class.
/// </remarks>
/// <param name="logger">The logger.</param>
public class PriceFilterHandler(ILogger<PriceFilterHandler> logger) : GameQueryHandlerBase
{
    private readonly ILogger<PriceFilterHandler> _logger = logger;

    /// <summary>
    /// Filters games by price range.
    /// </summary>
    /// <param name="games">The list of games to filter.</param>
    /// <param name="parameters">The filter parameters.</param>
    /// <returns>The filtered game filter result.</returns>
    public override async Task<GameFilterResult> HandleAsync(IEnumerable<Game> games, GameFilterParameters parameters)
    {
        _logger.LogInformation("Filtering games by price range");

        var filteredGames = games;

        if (parameters.MinPrice.HasValue)
        {
            _logger.LogInformation("Filtering games with price >= {MinPrice}", parameters.MinPrice.Value);
            filteredGames = filteredGames.Where(g => g.Price >= parameters.MinPrice.Value);
        }

        if (parameters.MaxPrice.HasValue)
        {
            _logger.LogInformation("Filtering games with price <= {MaxPrice}", parameters.MaxPrice.Value);
            filteredGames = filteredGames.Where(g => g.Price <= parameters.MaxPrice.Value);
        }

        return await PassToNextAsync(filteredGames, parameters);
    }
}