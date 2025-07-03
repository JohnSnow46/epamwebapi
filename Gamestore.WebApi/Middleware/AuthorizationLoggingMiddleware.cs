using Gamestore.WebApi.Extensions;

namespace Gamestore.WebApi.Middleware;

/// <summary>
/// Middleware for authorization
/// </summary>
public class AuthorizationLoggingMiddleware(RequestDelegate next, ILogger<AuthorizationLoggingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<AuthorizationLoggingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        LogUserInfo(context);

        await _next(context);
        LogAuthorizationResult(context);
    }

    private void LogUserInfo(HttpContext context)
    {
        var user = context.User;
        var request = context.Request;
        var endpoint = $"{request.Method} {request.Path}";

        if (user.Identity?.IsAuthenticated == true)
        {
            var userEmail = user.GetUserEmail();
            var userRole = user.GetUserRole();
            var userName = user.GetUserName();

            _logger.LogInformation("🔐 AUTH REQUEST: {Endpoint} | User: {Email} | Role: {Role} | Name: {Name}",
                endpoint, userEmail, userRole, userName);

            var claims = user.Claims.Select(c => $"{c.Type}={c.Value}").ToList();
            _logger.LogDebug("📋 User Claims: {Claims}", string.Join(", ", claims));
        }
        else
        {
            _logger.LogInformation("🔓 ANONYMOUS REQUEST: {Endpoint} | IP: {IP}",
                endpoint, context.Connection.RemoteIpAddress?.ToString() ?? "unknown");
        }
    }

    private void LogAuthorizationResult(HttpContext context)
    {
        var user = context.User;
        var request = context.Request;
        var response = context.Response;
        var endpoint = $"{request.Method} {request.Path}";

        var statusIcon = response.StatusCode switch
        {
            200 => "✅", // Success
            401 => "🚫", // Unauthorized
            403 => "⛔", // Forbidden
            404 => "❓", // Not Found
            >= 500 => "💥", // Server Error
            _ => "ℹ️"   // Other
        };

        if (user.Identity?.IsAuthenticated == true)
        {
            var userEmail = user.GetUserEmail();
            var userRole = user.GetUserRole();

            switch (response.StatusCode)
            {
                case 401:
                    _logger.LogWarning("{Icon} UNAUTHORIZED: {Endpoint} | User: {Email} | Role: {Role} | Status: {Status}",
                        statusIcon, endpoint, userEmail, userRole, response.StatusCode);
                    break;
                case 403:
                    _logger.LogWarning("{Icon} FORBIDDEN: {Endpoint} | User: {Email} | Role: {Role} | Status: {Status}",
                        statusIcon, endpoint, userEmail, userRole, response.StatusCode);
                    break;
                case >= 400:
                    _logger.LogWarning("{Icon} ERROR: {Endpoint} | User: {Email} | Role: {Role} | Status: {Status}",
                        statusIcon, endpoint, userEmail, userRole, response.StatusCode);
                    break;
                default:
                    _logger.LogInformation("{Icon} SUCCESS: {Endpoint} | User: {Email} | Role: {Role} | Status: {Status}",
                        statusIcon, endpoint, userEmail, userRole, response.StatusCode);
                    break;
            }
        }
        else
        {
            switch (response.StatusCode)
            {
                case 401:
                    _logger.LogInformation("{Icon} ANONYMOUS DENIED: {Endpoint} | Status: {Status} (Authentication required)",
                        statusIcon, endpoint, response.StatusCode);
                    break;
                case 403:
                    _logger.LogWarning("{Icon} ANONYMOUS FORBIDDEN: {Endpoint} | Status: {Status}",
                        statusIcon, endpoint, response.StatusCode);
                    break;
                default:
                    _logger.LogInformation("{Icon} ANONYMOUS ACCESS: {Endpoint} | Status: {Status}",
                        statusIcon, endpoint, response.StatusCode);
                    break;
            }
        }

        if (response.StatusCode == 403 && IsAdminEndpoint(request.Path))
        {
            _logger.LogWarning("🚨 SECURITY: Attempted access to admin endpoint {Endpoint} by {User}",
                endpoint, user.Identity?.IsAuthenticated == true ? user.GetUserEmail() : "Anonymous");
        }
    }

    private static bool IsAdminEndpoint(string path)
    {
        var adminPaths = new[]
        {
            "/api/users",
            "/api/roles",
            "/api/admin",
            "/api/games/deleted"
        };

        return adminPaths.Any(adminPath => path.StartsWith(adminPath, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Extension method do rejestracji middleware
/// </summary>
public static class AuthorizationLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseAuthorizationLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthorizationLoggingMiddleware>();
    }
}