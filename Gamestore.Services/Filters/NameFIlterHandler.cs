using Gamestore.Entities.Business;
using Gamestore.Services.Dto.FiltersDto;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services.Filters;
/// <summary>
/// Handles name filtering in the game query pipeline.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="NameFilterHandler"/> class.
/// </remarks>
/// <param name="logger">The logger.</param>
public class NameFilterHandler(ILogger<NameFilterHandler> logger) : GameQueryHandlerBase
{
    private readonly ILogger<NameFilterHandler> _logger = logger;

    /// <summary>
    /// Filters games by name.
    /// </summary>
    /// <param name="games">The list of games to filter.</param>
    /// <param name="parameters">The filter parameters.</param>
    /// <returns>The filtered game filter result.</returns>
    public override async Task<GameFilterResult> HandleAsync(IEnumerable<Game> games, GameFilterParameters parameters)
    {
        _logger.LogInformation("Filtering games by name");

        var filteredGames = games;

        if (!string.IsNullOrWhiteSpace(parameters.Name) && parameters.Name.Length >= 3)
        {
            _logger.LogInformation("Filtering games by name containing '{Name}'", parameters.Name);

            filteredGames = games.Where(g =>
                g.Name.Contains(parameters.Name, StringComparison.OrdinalIgnoreCase));
        }

        return await PassToNextAsync(filteredGames, parameters);
    }
}