using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Gamestore.WebApi.Controllers;
// DEBUGOWANIE - dodaj ten controller do sprawdzenia czy API działa

[ApiController]
[Route("api/debug")]
public class DebugController : ControllerBase
{
    private readonly ILogger<DebugController> _logger;

    public DebugController(ILogger<DebugController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Test endpoint - czy API w ogóle odpowiada
    /// </summary>
    [HttpGet("ping")]
    [AllowAnonymous]
    public IActionResult Ping()
    {
        _logger.LogInformation("Debug ping called");
        return Ok(new
        {
            message = "API is working!",
            timestamp = DateTime.UtcNow,
            endpoints = new[]
            {
                "GET /api/games/{key}",
                "GET /api/games/find/{id}",
                "GET /api/games-filter",
                "GET /api/unified-products",
                "GET /api/shippers"
            }
        });
    }

    /// <summary>
    /// Test czy games controller jest dostępny
    /// </summary>
    [HttpGet("games-test")]
    [AllowAnonymous]
    public IActionResult TestGamesEndpoint()
    {
        _logger.LogInformation("Testing games endpoint availability");
        return Ok(new
        {
            message = "Games endpoint route test",
            expectedUrls = new[]
            {
                "/api/games/test-key",
                "/api/games/find/00000000-0000-0000-0000-000000000001",
                "/api/games-filter"
            }
        });
    }

    /// <summary>
    /// Sprawdź wszystkie zarejestrowane endpointy
    /// </summary>
    [HttpGet("routes")]
    [AllowAnonymous]
    public IActionResult GetRoutes([FromServices] IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
    {
        var routes = actionDescriptorCollectionProvider.ActionDescriptors.Items
            .Where(x => x.AttributeRouteInfo != null)
            .Select(x => new
            {
                Action = x.RouteValues["Action"],
                Controller = x.RouteValues["Controller"],
                Template = x.AttributeRouteInfo.Template,
                HttpMethods = string.Join(", ", x.ActionConstraints?.OfType<HttpMethodActionConstraint>().FirstOrDefault()?.HttpMethods ?? Array.Empty<string>())
            })
            .OrderBy(x => x.Controller)
            .ThenBy(x => x.Template)
            .ToList();

        return Ok(routes);
    }
}