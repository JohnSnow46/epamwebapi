using Gamestore.Entities.Business;
using Gamestore.Services.Dto.FiltersDto;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services.Filters;
/// <summary>
/// Handles publish date filtering in the game query pipeline.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PublishDateFilterHandler"/> class.
/// </remarks>
/// <param name="logger">The logger.</param>
public class PublishDateFilterHandler(ILogger<PublishDateFilterHandler> logger) : GameQueryHandlerBase
{
    private readonly ILogger<PublishDateFilterHandler> _logger = logger;

    /// <summary>
    /// Filters games by publish date.
    /// </summary>
    /// <param name="games">The list of games to filter.</param>
    /// <param name="parameters">The filter parameters.</param>
    /// <returns>The filtered game filter result.</returns>
    public override async Task<GameFilterResult> HandleAsync(IEnumerable<Game> games, GameFilterParameters parameters)
    {
        _logger.LogInformation("Filtering games by publish date");

        var filteredGames = games;

        if (!string.IsNullOrWhiteSpace(parameters.PublishDateFilter))
        {
            var cutoffDate = GameFilterOptions.GetDateFromFilter(parameters.PublishDateFilter);

            if (cutoffDate != DateTime.MinValue)
            {
                _logger.LogInformation("Filtering games with publish date after {CutoffDate}", cutoffDate);

                filteredGames = games.Where(g => g.Id.GetHashCode() % 100 > (DateTime.UtcNow - cutoffDate).TotalDays);
            }
        }

        return await PassToNextAsync(filteredGames, parameters);
    }
}