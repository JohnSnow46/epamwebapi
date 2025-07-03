using Gamestore.Entities.Business;
using Gamestore.Services.Dto.FiltersDto;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services.Filters;
/// <summary>
/// Handles pagination in the game query pipeline.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PaginationHandler"/> class.
/// </remarks>
/// <param name="logger">The logger.</param>
public class PaginationHandler(ILogger<PaginationHandler> logger) : GameQueryHandlerBase
{
    private readonly ILogger<PaginationHandler> _logger = logger;

    /// <summary>
    /// Applies pagination to the list of games.
    /// </summary>
    /// <param name="games">The list of games to paginate.</param>
    /// <param name="parameters">The filter parameters.</param>
    /// <returns>The paginated game filter result.</returns>
    public override Task<GameFilterResult> HandleAsync(IEnumerable<Game> games, GameFilterParameters parameters)
    {
        _logger.LogInformation("Applying pagination with page size: {PageSize}, page: {Page}",
            parameters.PageSize, parameters.Page);
        var gamesList = games.ToList();
        var totalCount = gamesList.Count;
        var pageSize = GetPageSize(parameters.PageSize);
        if (pageSize == 0)
        {
            return Task.FromResult(new GameFilterResult
            {
                Games = gamesList.Select(MapToGameDto).ToList(),
                CurrentPage = 1,
                TotalPages = 1
            });
        }
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        var currentPage = Math.Min(Math.Max(1, parameters.Page), totalPages);
        var paginatedGames = gamesList
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize);
        _logger.LogInformation("Pagination applied. Total games: {TotalCount}, Page size: {PageSize}, " +
                               "Total pages: {TotalPages}, Current page: {CurrentPage}",
            totalCount, pageSize, totalPages, currentPage);
        return Task.FromResult(new GameFilterResult
        {
            Games = paginatedGames.Select(MapToGameDto).ToList(),
            CurrentPage = currentPage,
            TotalPages = totalPages
        });
    }

    /// <summary>
    /// Gets the page size from the page size parameter.
    /// </summary>
    /// <param name="pageSizeParam">The page size parameter.</param>
    /// <returns>The page size as an integer.</returns>
    private static int GetPageSize(string? pageSizeParam)
    {
        if (string.IsNullOrWhiteSpace(pageSizeParam))
        {
            return 10; // Default page size
        }

        if (pageSizeParam.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            return 0; // Return all games
        }

        if (int.TryParse(pageSizeParam, out var pageSize) && pageSize > 0)
        {
            return pageSize;
        }

        return 10; // Default page size
    }
}