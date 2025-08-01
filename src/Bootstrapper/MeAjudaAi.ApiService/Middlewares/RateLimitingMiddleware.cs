using MeAjudaAi.ApiService.Options;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace MeAjudaAi.ApiService.Middlewares;

public class RateLimitingMiddleware(
    RequestDelegate next,
    IMemoryCache cache,
    RateLimitOptions options)
{
    private readonly RequestDelegate _next = next;
    private readonly IMemoryCache _cache = cache;
    private readonly RateLimitOptions _options = options;
    private readonly Serilog.ILogger _logger = Log.ForContext<RateLimitingMiddleware>();

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = GetClientIpAddress(context);
        var endpoint = $"{context.Request.Method}:{context.Request.Path}";
        var key = $"rate_limit_{clientIp}_{endpoint}";

        var config = GetRateLimitConfig(context);

        if (!_cache.TryGetValue(key, out int requestCount))
        {
            requestCount = 0;
        }

        if (requestCount >= config.RequestsPerWindow)
        {
            _logger.Warning(
                "Rate limit exceeded for {ClientIp} on {Endpoint}. Count: {RequestCount}/{Limit}",
                clientIp, endpoint, requestCount, config.RequestsPerWindow);

            context.Response.StatusCode = 429;
            context.Response.Headers.Append("Retry-After", config.WindowInSeconds.ToString());

            await context.Response.WriteAsync("Rate limit exceeded. Try again later.");
            return;
        }

        _cache.Set(key, requestCount + 1, TimeSpan.FromSeconds(config.WindowInSeconds));

        context.Response.Headers.Append("X-RateLimit-Limit", config.RequestsPerWindow.ToString());
        context.Response.Headers.Append("X-RateLimit-Remaining", (config.RequestsPerWindow - requestCount - 1).ToString());

        await _next(context);
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xForwardedFor))
            return xForwardedFor.Split(',')[0].Trim();

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private RateLimitConfig GetRateLimitConfig(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant();

        return path switch
        {
            var p when p?.Contains("/auth/") == true =>
                new RateLimitConfig(_options.AuthRequestsPerMinute, _options.WindowInSeconds),
            var p when p?.Contains("/search") == true =>
                new RateLimitConfig(_options.SearchRequestsPerMinute, _options.WindowInSeconds),
            _ => new RateLimitConfig(_options.DefaultRequestsPerMinute, _options.WindowInSeconds)
        };
    }

    private record RateLimitConfig(int RequestsPerWindow, int WindowInSeconds);
}