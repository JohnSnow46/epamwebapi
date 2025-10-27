namespace Gamestore.WebApi.Middleware;

/// <summary>
/// Extension method to register the middleware.
/// </summary>
public static class AuthHeaderFixMiddlewareExtensions
{
    public static IApplicationBuilder UseAuthHeaderFix(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthHeaderFixMiddleware>();
    }
}