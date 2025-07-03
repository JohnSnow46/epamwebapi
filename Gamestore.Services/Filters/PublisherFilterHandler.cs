using Gamestore.Entities.Business;
using Gamestore.Services.Dto.FiltersDto;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services.Filters;
/// <summary>
/// Handles publisher filtering in the game query pipeline.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PublisherFilterHandler"/> class.
/// </remarks>
/// <param name="logger">The logger.</param>
public class PublisherFilterHandler(ILogger<PublisherFilterHandler> logger) : GameQueryHandlerBase
{
    private readonly ILogger<PublisherFilterHandler> _logger = logger;

    /// <summary>
    /// Filters games by publisher IDs.
    /// </summary>
    /// <param name="games">The list of games to filter.</param>
    /// <param name="parameters">The filter parameters.</param>
    /// <returns>The filtered game filter result.</returns>
    public override async Task<GameFilterResult> HandleAsync(IEnumerable<Game> games, GameFilterParameters parameters)
    {
        _logger.LogInformation("Filtering games by publishers");

        var filteredGames = games;

        if (parameters.PublisherIds != null && parameters.PublisherIds.Count != 0)
        {
            _logger.LogInformation("Applying publisher filter with {Count} publisher IDs", parameters.PublisherIds.Count);

            filteredGames = games.Where(g =>
                g.PublisherId.HasValue &&
                parameters.PublisherIds.Contains(g.PublisherId.Value));
        }

        return await PassToNextAsync(filteredGames, parameters);
    }
}
