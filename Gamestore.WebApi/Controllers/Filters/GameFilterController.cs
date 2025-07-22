using Gamestore.Entities.ErrorModels;
using Gamestore.Services.Dto.FiltersDto;
using Gamestore.Services.Dto.GamesDto;
using Gamestore.Services.Interfaces;
using Gamestore.Services.Services.Auth;
using Gamestore.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebApi.Controllers.Filters;

/// <summary>
/// Enhanced GameFilterController z obsługą MongoDB przez UnifiedProductService
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
    /// Debug endpoint - sprawdź klucze produktów
    /// </summary>
    [HttpGet("debug/keys")]
    [AllowAnonymous]
    public async Task<IActionResult> DebugProductKeys()
    {
        try
        {
            // Pobierz unified products
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
    /// 🔥 ZMODYFIKOWANY ENDPOINT: Pobiera dane z obu baz (SQL + MongoDB)
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

            // OPCJA 1: Użyj standardowego GameFilterService (tylko SQL)
            // var result = await _gameFilterService.GetFilteredGamesAsync(parameters);

            // OPCJA 2: Użyj UnifiedProductService + konwersja na GameFilterResult
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
    /// Konwertuje unified products na GameFilterResult z filtrami
    /// </summary>
    private GameFilterResult ConvertUnifiedProductsToGameFilterResult(IEnumerable<object> unifiedProducts, GameFilterParameters parameters)
    {
        var games = new List<GameUpdateRequestDto>();

        // Konwertuj na GameUpdateRequestDto
        foreach (var product in unifiedProducts)
        {
            try
            {
                // Użyj reflection do odczytania właściwości z anonimowego obiektu
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

        // Zastosuj filtry
        var filteredGames = ApplyFilters(games, parameters);

        // Zastosuj sortowanie
        var sortedGames = ApplySorting(filteredGames, parameters);

        // Zastosuj paginację
        var paginatedResult = ApplyPagination(sortedGames, parameters);

        return new GameFilterResult
        {
            Games = paginatedResult.games,
            CurrentPage = paginatedResult.currentPage,
            TotalPages = paginatedResult.totalPages
        };
    }

    /// <summary>
    /// Pobiera wartość właściwości z obiektu używając reflection
    /// </summary>
    private T GetPropertyValue<T>(object obj, Type objType, string propertyName)
    {
        try
        {
            var property = objType.GetProperty(propertyName);
            if (property != null)
            {
                var value = property.GetValue(obj);
                if (value is T typedValue)
                {
                    return typedValue;
                }

                // Spróbuj konwersji
                if (value != null && typeof(T) != typeof(string))
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }

                if (typeof(T) == typeof(string))
                {
                    return (T)(object)(value?.ToString() ?? string.Empty);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to get property {PropertyName} from object", propertyName);
        }

        return default(T);
    }

    // Safe conversion helper methods
    private static double GetSafeDouble(object value)
    {
        if (value == null) return 0.0;
        if (value is double d) return d;
        if (value is decimal dec) return (double)dec;
        if (value is int i) return i;
        if (double.TryParse(value.ToString(), out var parsed)) return parsed;
        return 0.0;
    }

    private static int GetSafeInt(object value)
    {
        if (value == null) return 0;
        if (value is int i) return i;
        if (value is double d) return (int)d;
        if (int.TryParse(value.ToString(), out var parsed)) return parsed;
        return 0;
    }

    private static bool GetSafeBool(object value)
    {
        if (value == null) return false;
        if (value is bool b) return b;
        if (value.ToString().ToLowerInvariant() == "true") return true;
        if (int.TryParse(value.ToString(), out var intVal)) return intVal != 0;
        return false;
    }

    private IEnumerable<GameUpdateRequestDto> ApplyFilters(IEnumerable<GameUpdateRequestDto> games, GameFilterParameters parameters)
    {
        var filtered = games;

        // Filtr nazwy
        if (!string.IsNullOrWhiteSpace(parameters.Name))
        {
            filtered = filtered.Where(g => g.Name.Contains(parameters.Name, StringComparison.OrdinalIgnoreCase));
        }

        // Filtr ceny
        if (parameters.MinPrice.HasValue)
        {
            filtered = filtered.Where(g => g.Price >= parameters.MinPrice.Value);
        }

        if (parameters.MaxPrice.HasValue)
        {
            filtered = filtered.Where(g => g.Price <= parameters.MaxPrice.Value);
        }

        // TODO: Dodaj filtry dla genres, platforms, publishers gdy będą dostępne w unified data

        return filtered;
    }

    private IEnumerable<GameUpdateRequestDto> ApplySorting(IEnumerable<GameUpdateRequestDto> games, GameFilterParameters parameters)
    {
        if (string.IsNullOrWhiteSpace(parameters.SortBy))
            return games;

        return parameters.SortBy.ToLowerInvariant() switch
        {
            "price asc" => games.OrderBy(g => g.Price),
            "price desc" => games.OrderByDescending(g => g.Price),
            "most popular" => games.OrderByDescending(g => g.Name), // Fallback - brak viewCount w GameUpdateRequestDto
            "new" => games.OrderByDescending(g => g.Name),
            _ => games
        };
    }

    private (List<GameUpdateRequestDto> games, int currentPage, int totalPages) ApplyPagination(IEnumerable<GameUpdateRequestDto> games, GameFilterParameters parameters)
    {
        var gamesList = games.ToList();
        var pageSize = GetPageSize(parameters.PageSize);
        var currentPage = parameters.Page; // Page jest już int z domyślną wartością 1

        if (pageSize <= 0) // "all"
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

    // Helper methods
    private static Guid GetGuidFromProduct(IDictionary<string, object> product, string key)
    {
        if (product.TryGetValue(key, out var value))
        {
            if (value is string str && Guid.TryParse(str, out var guid))
                return guid;
        }
        return Guid.NewGuid();
    }

    private static string GetStringFromProduct(IDictionary<string, object> product, string key)
    {
        if (product.TryGetValue(key, out var value))
        {
            var result = value?.ToString();
            return string.IsNullOrEmpty(result) ? GetDefaultValue(key) : result;
        }
        return GetDefaultValue(key);
    }

    private static string GetDefaultValue(string key)
    {
        return key switch
        {
            "name" => "Unknown Product",
            "key" => Guid.NewGuid().ToString(),
            "description" => "No description available",
            _ => string.Empty
        };
    }

    private static double GetDoubleFromProduct(IDictionary<string, object> product, string key)
    {
        if (product.TryGetValue(key, out var value))
        {
            if (value is double d) return d;
            if (value is decimal dec) return (double)dec;
            if (double.TryParse(value?.ToString(), out var parsed)) return parsed;
        }
        return 0.0;
    }

    private static int GetIntFromProduct(IDictionary<string, object> product, string key)
    {
        if (product.TryGetValue(key, out var value))
        {
            if (value is int i) return i;
            if (int.TryParse(value?.ToString(), out var parsed)) return parsed;
        }
        return 0;
    }

    private static bool GetBoolFromProduct(IDictionary<string, object> product, string key)
    {
        if (product.TryGetValue(key, out var value))
        {
            if (value is bool b) return b;
            if (bool.TryParse(value?.ToString(), out var parsed)) return parsed;
        }
        return false;
    }

    private static int GetPageSize(string pageSizeStr)
    {
        if (string.IsNullOrWhiteSpace(pageSizeStr) || pageSizeStr.ToLowerInvariant() == "all")
            return 0;

        return int.TryParse(pageSizeStr, out var pageSize) && pageSize > 0 ? pageSize : 10;
    }

    // Pozostałe endpointy bez zmian...
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
}

/// <summary>
/// Extension methods dla GameFilterController
/// </summary>
public static class ObjectExtensions
{
    public static TResult Let<T, TResult>(this T obj, Func<T, TResult> func)
    {
        return func(obj);
    }
}