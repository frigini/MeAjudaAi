using MeAjudaAi.Gateway.Options;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.Gateway.Middlewares;

public class EdgeAuthGuardMiddleware
{
    private readonly RequestDelegate _next;
    private readonly EdgeAuthGuardOptions _options;
    private readonly ILogger<EdgeAuthGuardMiddleware> _logger;
    private readonly PathString[] _publicPathPrefixes;

    public EdgeAuthGuardMiddleware(
        RequestDelegate next,
        IOptions<EdgeAuthGuardOptions> options,
        ILogger<EdgeAuthGuardMiddleware> logger)
    {
        _next = next;
        _options = options.Value;
        _logger = logger;

        _publicPathPrefixes = _options.PublicRoutes
            .Select(p => new PathString(p))
            .ToArray();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_options.Enabled)
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;
        var pathString = new PathString(path);

        if (!pathString.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var isPublicRoute = false;
        foreach (var publicPrefix in _publicPathPrefixes)
        {
            if (pathString.StartsWithSegments(publicPrefix, StringComparison.OrdinalIgnoreCase))
            {
                isPublicRoute = true;
                break;
            }
        }

        context.Items["X-Gateway-PublicRoute"] = isPublicRoute;

        var isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;

        if (!isPublicRoute && !isAuthenticated)
        {
            _logger.LogWarning(
                "Edge auth guard blocked request to {Path} from {IpAddress}",
                path,
                context.Connection.RemoteIpAddress);

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.Headers[_options.ChallengeHeader] = "true";
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                error = "authentication_required",
                message = "Authentication required. Please provide a valid token.",
                publicRoutes = _options.PublicRoutes
            };

            await context.Response.WriteAsJsonAsync(errorResponse);
            return;
        }

        if (isAuthenticated)
        {
            context.Response.Headers[_options.AuthenticatedHeader] = "true";
        }

        await _next(context);
    }
}

public static class EdgeAuthGuardMiddlewareExtensions
{
    public static IApplicationBuilder UseEdgeAuthGuard(this IApplicationBuilder app)
    {
        return app.UseMiddleware<EdgeAuthGuardMiddleware>();
    }
}