using Gamestore.Data.Interfaces;
using Gamestore.Services.Dto.FiltersDto;
using Gamestore.Services.Filters;
using Gamestore.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services.Services.Filters;

/// <summary>
/// Service for filtering and managing game queries.
/// </summary>
public class GameFilterService : IGameFilterService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GameFilterService> _logger;

    // Only keep the first handler in the chain as a field
    private readonly GenreFilterHandler _firstHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameFilterService"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public GameFilterService(
        IUnitOfWork unitOfWork,
        ILoggerFactory loggerFactory)
    {
        _unitOfWork = unitOfWork;
        _logger = loggerFactory.CreateLogger<GameFilterService>();

        // Initialize handlers as local variables
        var genreFilterHandler = new GenreFilterHandler(loggerFactory.CreateLogger<GenreFilterHandler>());
        var platformFilterHandler = new PlatformFilterHandler(loggerFactory.CreateLogger<PlatformFilterHandler>());
        var publisherFilterHandler = new PublisherFilterHandler(loggerFactory.CreateLogger<PublisherFilterHandler>());
        var priceFilterHandler = new PriceFilterHandler(loggerFactory.CreateLogger<PriceFilterHandler>());
        var nameFilterHandler = new NameFilterHandler(loggerFactory.CreateLogger<NameFilterHandler>());
        var publishDateFilterHandler = new PublishDateFilterHandler(loggerFactory.CreateLogger<PublishDateFilterHandler>());
        var sortingHandler = new SortingHandler(loggerFactory.CreateLogger<SortingHandler>());
        var paginationHandler = new PaginationHandler(loggerFactory.CreateLogger<PaginationHandler>());

        // Chain the handlers together
        genreFilterHandler
            .SetNext(platformFilterHandler)
            .SetNext(publisherFilterHandler)
            .SetNext(priceFilterHandler)
            .SetNext(nameFilterHandler)
            .SetNext(publishDateFilterHandler)
            .SetNext(sortingHandler)
            .SetNext(paginationHandler);

        // Store only the first handler in the chain
        _firstHandler = genreFilterHandler;
    }

    /// <summary>
    /// Gets the pagination options.
    /// </summary>
    /// <returns>The list of pagination options.</returns>
    public List<string> GetPaginationOptions()
    {
        return GameFilterOptions.PaginationOptions.ToList();
    }

    /// <summary>
    /// Gets the sorting options.
    /// </summary>
    /// <returns>The list of sorting options.</returns>
    public List<string> GetSortingOptions()
    {
        return GameFilterOptions.SortingOptions.ToList();
    }

    /// <summary>
    /// Gets the publishing date filter options.
    /// </summary>
    /// <returns>The list of publishing date filter options.</returns>
    public List<string> GetPublishDateFilterOptions()
    {
        return GameFilterOptions.PublishDateOptions.ToList();
    }

    /// <summary>
    /// Gets filtered games based on the specified parameters.
    /// </summary>
    /// <param name="parameters">The filter parameters.</param>
    /// <returns>The filtered game result.</returns>
    public async Task<GameFilterResult> GetFilteredGamesAsync(GameFilterParameters parameters)
    {
        _logger.LogInformation("Getting filtered games with parameters: {@Parameters}", parameters);
        var games = await _unitOfWork.Games.GetAllAsync();

        return await _firstHandler.HandleAsync(games, parameters);
    }

    /// <summary>
    /// Increments the view count for a game by its key.
    /// </summary>
    /// <param name="gameKey">The game key.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task IncrementGameViewCountAsync(string gameKey)
    {
        _logger.LogInformation("Incrementing view count for game with key: {GameKey}", gameKey);
        var game = await _unitOfWork.Games.GetKeyAsync(gameKey);

        if (game == null)
        {
            _logger.LogWarning("Game with key: {GameKey} not found", gameKey);
            throw new KeyNotFoundException($"Game with key '{gameKey}' not found");
        }

        await _unitOfWork.Games.IncrementViewCountAsync(game.Key);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("View count incremented successfully for game with key: {GameKey}", gameKey);
    }
}