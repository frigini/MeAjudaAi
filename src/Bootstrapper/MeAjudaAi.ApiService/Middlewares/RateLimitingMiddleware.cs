using MeAjudaAi.ApiService.Options;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace MeAjudaAi.ApiService.Middlewares;

/// <summary>
/// Middleware de Rate Limiting com suporte a usuários autenticados
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly RateLimitOptions _options;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    public RateLimitingMiddleware(
        RequestDelegate next,
        IMemoryCache cache,
        RateLimitOptions options,
        ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _options = options;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = GetClientIpAddress(context);
        var isAuthenticated = context.User.Identity?.IsAuthenticated == true;
        
        var limit = isAuthenticated ? _options.Authenticated.RequestsPerMinute : _options.Anonymous.RequestsPerMinute;
        
        var key = $"rate_limit:{clientIp}:{context.Request.Path}";
        
        if (!_cache.TryGetValue(key, out int requestCount))
        {
            requestCount = 0;
        }

        if (requestCount >= limit)
        {
            await HandleRateLimitExceeded(context, limit);
            return;
        }

        _cache.Set(key, requestCount + 1, TimeSpan.FromMinutes(1));
        await _next(context);
    }

    private string GetClientIpAddress(HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private async Task HandleRateLimitExceeded(HttpContext context, int limit)
    {
        context.Response.StatusCode = 429;
        context.Response.Headers.Append("Retry-After", "60");
        context.Response.ContentType = "application/json";

        var errorResponse = new 
        {
            Error = "RateLimitExceeded",
            Message = "Rate limit exceeded. Please try again later.",
            Details = new Dictionary<string, object>
            {
                ["limit"] = limit,
                ["retryAfterSeconds"] = 60
            }
        };

        var json = JsonSerializer.Serialize(errorResponse, SerializationDefaults.Api);

        await context.Response.WriteAsync(json);
    }
}