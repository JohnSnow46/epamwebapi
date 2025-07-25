using Gamestore.Entities.ErrorModels;
using Gamestore.Services.Dto.FiltersDto;
using Gamestore.Services.Dto.GamesDto;
using Gamestore.Services.Interfaces;
using Gamestore.Services.Services.Auth;
using Gamestore.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebApi.Controllers.Filters;
#pragma warning disable S3358

/// <summary>
/// Enhanced GameFilterController with MongoDB support through UnifiedProductService
/// </summary>
[ApiController]
[Route("api/games-filter")]
public class GameFilterController(
    IGameFilterService gameFilterService,
    IUnifiedProductService unifiedProductService,
    ILogger<GameFilterController> logger) : ControllerBase
{
    private readonly IGameFilterService _gameFilterService = gameFilterService;
    private readonly IUnifiedProductService _unifiedProductService = unifiedProductService;
    private readonly ILogger<GameFilterController> _logger = logger;

    /// <summary>
    /// Debug endpoint for checking product keys
    /// </summary>
    [HttpGet("debug/keys")]
    [AllowAnonymous]
    public async Task<IActionResult> DebugProductKeys()
    {
        try
        {
            var products = await _unifiedProductService.GetAllProductsAsync();

            var productInfo = new List<object>();

            foreach (var product in products)
            {
                var productType = product.GetType();
                var id = GetPropertyValue<object>(product, productType, "id");
                var name = GetPropertyValue<object>(product, productType, "name");
                var key = GetPropertyValue<object>(product, productType, "key");
                var source = GetPropertyValue<object>(product, productType, "source");

                productInfo.Add(new
                {
                    id = id?.ToString(),
                    name = name?.ToString(),
                    key = key?.ToString(),
                    source = source?.ToString(),
                    hasValidKey = !string.IsNullOrEmpty(key?.ToString()),
                    keyLength = key?.ToString()?.Length ?? 0
                });
            }

            var sqlProducts = productInfo.Where(p => p.GetType().GetProperty("source")?.GetValue(p)?.ToString() == "SQL");
            var mongoProducts = productInfo.Where(p => p.GetType().GetProperty("source")?.GetValue(p)?.ToString() == "MongoDB");

            return Ok(new
            {
                total_products = productInfo.Count,
                sql_count = sqlProducts.Count(),
                mongo_count = mongoProducts.Count(),
                products_with_keys = productInfo.Count(p =>
                {
                    var hasValidKey = p.GetType().GetProperty("hasValidKey")?.GetValue(p);
                    return hasValidKey is bool b && b;
                }),
                sample_products = productInfo.Take(10)
            });
        }
        catch (Exception ex)
        {
            return Ok(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets filtered games from both databases (SQL + MongoDB)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetFilteredGames([FromQuery] GameFilterParameters parameters)
    {
        try
        {
            var userEmail = User.Identity?.IsAuthenticated == true ? User.GetUserEmail() : "Anonymous";
            var userRole = User.Identity?.IsAuthenticated == true ? User.GetUserRole() : Roles.Guest;

            _logger.LogInformation("Getting unified filtered games with parameters: {@Parameters} for user: {User} with role: {Role}",
                parameters, userEmail, userRole);

            var unifiedProducts = await _unifiedProductService.GetAllProductsAsync();
            var result = ConvertUnifiedProductsToGameFilterResult(unifiedProducts, parameters);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error retrieving filtered games");
        }
    }

    /// <summary>
    /// Gets pagination options
    /// </summary>
    [HttpGet("pagination-options")]
    [AllowAnonymous]
    public IActionResult GetPaginationOptions()
    {
        try
        {
            _logger.LogInformation("Getting pagination options");
            var options = _gameFilterService.GetPaginationOptions();
            return Ok(options);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error retrieving pagination options");
        }
    }

    /// <summary>
    /// Gets sorting options
    /// </summary>
    [HttpGet("sorting-options")]
    [AllowAnonymous]
    public IActionResult GetSortingOptions()
    {
        try
        {
            _logger.LogInformation("Getting sorting options");
            var options = _gameFilterService.GetSortingOptions();
            return Ok(options);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error retrieving sorting options");
        }
    }

    /// <summary>
    /// Gets publish date filter options
    /// </summary>
    [HttpGet("publish-date-options")]
    [AllowAnonymous]
    public IActionResult GetPublishDateOptions()
    {
        try
        {
            _logger.LogInformation("Getting publish date filter options");
            var options = _gameFilterService.GetPublishDateFilterOptions();
            return Ok(options);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error retrieving publish date filter options");
        }
    }

    private GameFilterResult ConvertUnifiedProductsToGameFilterResult(IEnumerable<object> unifiedProducts, GameFilterParameters parameters)
    {
        var games = new List<GameUpdateRequestDto>();

        foreach (var product in unifiedProducts)
        {
            try
            {
                var productType = product.GetType();

                var gameDto = new GameUpdateRequestDto
                {
                    Id = GetPropertyValue<string>(product, productType, "id")?.Let(s => Guid.TryParse(s, out var g) ? g : Guid.NewGuid()) ?? Guid.NewGuid(),
                    Name = GetPropertyValue<string>(product, productType, "name") ?? "Unknown Product",
                    Key = GetPropertyValue<string>(product, productType, "key") ?? Guid.NewGuid().ToString(),
                    Description = GetPropertyValue<string>(product, productType, "description") ?? "No description",
                    Price = GetSafeDouble(GetPropertyValue<object>(product, productType, "price")),
                    UnitInStock = GetSafeInt(GetPropertyValue<object>(product, productType, "unitsInStock")),
                    Discontinued = GetSafeBool(GetPropertyValue<object>(product, productType, "discontinued")) ? 1 : 0
                };

                games.Add(gameDto);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to convert product to GameUpdateRequestDto");
            }
        }

        _logger.LogInformation("Converted {Count} unified products to GameUpdateRequestDto", games.Count);

        var filteredGames = ApplyFilters(games, parameters);
        var sortedGames = ApplySorting(filteredGames, parameters);
        var paginatedResult = ApplyPagination(sortedGames, parameters);

        return new GameFilterResult
        {
            Games = paginatedResult.games,
            CurrentPage = paginatedResult.currentPage,
            TotalPages = paginatedResult.totalPages
        };
    }

    private T GetPropertyValue<T>(object obj, Type objType, string propertyName)
    {
        try
        {
            var property = objType.GetProperty(propertyName);
            if (property == null)
            {
                return default;
            }

            var value = property.GetValue(obj);

            // Direct type match
            if (value is T typedValue)
            {
                return typedValue;
            }

            // Handle string conversion specifically
            if (typeof(T) == typeof(string))
            {
                return ConvertToString<T>(value);
            }

            // Handle other type conversions
            return value != null ? (T)Convert.ChangeType(value, typeof(T)) : default;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to get property {PropertyName} from object", propertyName);
            return default;
        }
    }

    private static T ConvertToString<T>(object? value)
    {
        var stringValue = value?.ToString() ?? string.Empty;
        return (T)(object)stringValue;
    }

    private static double GetSafeDouble(object value)
    {
        return value == null
            ? 0.0
            : value is double d
            ? d
            : value is decimal dec ? (double)dec : value is int i ? i : double.TryParse(value.ToString(), out var parsed) ? parsed : 0.0;
    }

    private static int GetSafeInt(object value)
    {
        return value == null
            ? 0
            : value is int i ? i : value is double d ? (int)d : int.TryParse(value.ToString(), out var parsed) ? parsed : 0;
    }

    private static bool GetSafeBool(object value)
    {
        return value != null && (value is bool b
            ? b
            : value.ToString().ToLowerInvariant() == "true" || (int.TryParse(value.ToString(), out var intVal) && intVal != 0));
    }

    private static IEnumerable<GameUpdateRequestDto> ApplyFilters(IEnumerable<GameUpdateRequestDto> games, GameFilterParameters parameters)
    {
        var filtered = games;

        if (!string.IsNullOrWhiteSpace(parameters.Name))
        {
            filtered = filtered.Where(g => g.Name.Contains(parameters.Name, StringComparison.OrdinalIgnoreCase));
        }

        if (parameters.MinPrice.HasValue)
        {
            filtered = filtered.Where(g => g.Price >= parameters.MinPrice.Value);
        }

        if (parameters.MaxPrice.HasValue)
        {
            filtered = filtered.Where(g => g.Price <= parameters.MaxPrice.Value);
        }

        return filtered;
    }

    private static IEnumerable<GameUpdateRequestDto> ApplySorting(IEnumerable<GameUpdateRequestDto> games, GameFilterParameters parameters)
    {
        return string.IsNullOrWhiteSpace(parameters.SortBy)
            ? games
            : parameters.SortBy.ToLowerInvariant() switch
            {
                "price asc" => games.OrderBy(g => g.Price),
                "price desc" => games.OrderByDescending(g => g.Price),
                "most popular" => games.OrderByDescending(g => g.Name),
                "new" => games.OrderByDescending(g => g.Name),
                _ => games
            };
    }

    private static (List<GameUpdateRequestDto> games, int currentPage, int totalPages) ApplyPagination(IEnumerable<GameUpdateRequestDto> games, GameFilterParameters parameters)
    {
        var gamesList = games.ToList();
        var pageSize = GetPageSize(parameters.PageSize ?? string.Empty);
        var currentPage = parameters.Page;

        if (pageSize <= 0)
        {
            return (gamesList, 1, 1);
        }

        var totalPages = (int)Math.Ceiling((double)gamesList.Count / pageSize);
        currentPage = Math.Max(1, Math.Min(currentPage, totalPages));

        var paginatedGames = gamesList
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (paginatedGames, currentPage, totalPages);
    }

    private static int GetPageSize(string pageSizeStr)
    {
        return string.IsNullOrWhiteSpace(pageSizeStr) || pageSizeStr.ToLowerInvariant() == "all"
            ? 0
            : int.TryParse(pageSizeStr, out var pageSize) && pageSize > 0 ? pageSize : 10;
    }

    private ObjectResult HandleException(Exception ex, string logMessage)
    {
        _logger.LogError(ex, "{LogMessage}: {ErrorMessage}", logMessage, ex.Message);
        return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponseModel
        {
            Message = "An error occurred.",
            Details = ex.Message,
            StatusCode = StatusCodes.Status500InternalServerError,
        });
    }

#pragma warning restore S3358
}

/// <summary>
/// Extension methods for GameFilterController
/// </summary>
public static class ObjectExtensions
{
    public static TResult Let<T, TResult>(this T obj, Func<T, TResult> func)
    {
        return func(obj);
    }
}