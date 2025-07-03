namespace Gamestore.WebApi.Middleware;

/// <summary>
/// Middleware to fix missing "Bearer " prefix in Authorization header
/// Some frontend frameworks send tokens without the Bearer prefix
/// </summary>
public class AuthHeaderFixMiddleware(RequestDelegate next, ILogger<AuthHeaderFixMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<AuthHeaderFixMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("Authorization", out var authHeaderValues))
        {
            var authHeader = authHeaderValues.FirstOrDefault();

            if (!string.IsNullOrEmpty(authHeader) && IsJwtTokenWithoutBearer(authHeader))
            {
                // Add "Bearer " prefix
                var fixedHeader = $"Bearer {authHeader}";
                context.Request.Headers.Authorization = fixedHeader;

                _logger.LogDebug("🔧 Fixed Authorization header: added 'Bearer ' prefix for token starting with: {TokenStart}",
                    authHeader[..Math.Min(10, authHeader.Length)]);
            }
        }

        await _next(context);
    }

    /// <summary>
    /// Checks if the authorization header contains a JWT token without "Bearer " prefix
    /// </summary>
    private static bool IsJwtTokenWithoutBearer(string authHeader)
    {
        return authHeader.StartsWith("eyJ", StringComparison.Ordinal) &&
               !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Extension method to register the middleware
/// </summary>
public static class AuthHeaderFixMiddlewareExtensions
{
    public static IApplicationBuilder UseAuthHeaderFix(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthHeaderFixMiddleware>();
    }
}