using Gamestore.Entities.Business;
using Gamestore.Services.Dto.FiltersDto;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services.Filters;
/// <summary>
/// Handles platform filtering in the game query pipeline.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PlatformFilterHandler"/> class.
/// </remarks>
/// <param name="logger">The logger.</param>
public class PlatformFilterHandler(ILogger<PlatformFilterHandler> logger) : GameQueryHandlerBase
{
    private readonly ILogger<PlatformFilterHandler> _logger = logger;

    /// <summary>
    /// Filters games by platform IDs.
    /// </summary>
    /// <param name="games">The list of games to filter.</param>
    /// <param name="parameters">The filter parameters.</param>
    /// <returns>The filtered game filter result.</returns>
    public override async Task<GameFilterResult> HandleAsync(IEnumerable<Game> games, GameFilterParameters parameters)
    {
        _logger.LogInformation("Filtering games by platforms");

        var filteredGames = games;

        if (parameters.PlatformIds != null && parameters.PlatformIds.Count != 0)
        {
            _logger.LogInformation("Applying platform filter with {Count} platform IDs", parameters.PlatformIds.Count);

            filteredGames = games.Where(g =>
                g.GamePlatforms != null &&
                g.GamePlatforms.Any(gp => parameters.PlatformIds.Contains(gp.PlatformId)));
        }

        return await PassToNextAsync(filteredGames, parameters);
    }
}
