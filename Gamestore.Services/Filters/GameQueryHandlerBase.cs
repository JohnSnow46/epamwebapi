using Gamestore.Entities.Business;
using Gamestore.Services.Dto.FiltersDto;
using Gamestore.Services.Dto.GamesDto;
using Gamestore.Services.Filters.IFilters;

namespace Gamestore.Services.Filters;
/// <summary>
/// Base class for implementing game query handlers.
/// </summary>
public abstract class GameQueryHandlerBase : IGameQueryHandler
{
    private protected IGameQueryHandler? _nextHandler;

    /// <summary>
    /// Sets the next handler in the pipeline.
    /// </summary>
    /// <param name="handler">The next handler.</param>
    /// <returns>The next handler.</returns>
    public IGameQueryHandler SetNext(IGameQueryHandler handler)
    {
        _nextHandler = handler;
        return handler;
    }

    /// <summary>
    /// Handles the current processing step and passes to the next handler.
    /// </summary>
    /// <param name="games">The list of games to process.</param>
    /// <param name="parameters">The filter parameters.</param>
    /// <returns>The processed game filter result.</returns>
    public abstract Task<GameFilterResult> HandleAsync(IEnumerable<Game> games, GameFilterParameters parameters);

    /// <summary>
    /// Passes the processing to the next handler if one exists.
    /// </summary>
    /// <param name="games">The list of games to process.</param>
    /// <param name="parameters">The filter parameters.</param>
    /// <returns>The processed game filter result.</returns>
    protected async Task<GameFilterResult> PassToNextAsync(IEnumerable<Game> games, GameFilterParameters parameters)
    {
        return _nextHandler != null
            ? await _nextHandler.HandleAsync(games, parameters)
            : new GameFilterResult
            {
                Games = games.Select(MapToGameDto).ToList(),
                CurrentPage = parameters.Page,
                TotalPages = 1
            };
    }

    /// <summary>
    /// Maps a game entity to a game DTO.
    /// </summary>
    /// <param name="game">The game entity.</param>
    /// <returns>The game DTO.</returns>
    protected static GameUpdateRequestDto MapToGameDto(Game game)
    {
        return new GameUpdateRequestDto
        {
            Id = game.Id,
            Name = game.Name,
            Key = game.Key,
            Description = game.Description,
            Price = game.Price,
            UnitInStock = game.UnitInStock,
            Discontinued = game.Discontinued
        };
    }
}
