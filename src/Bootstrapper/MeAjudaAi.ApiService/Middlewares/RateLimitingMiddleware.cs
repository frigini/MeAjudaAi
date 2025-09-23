using MeAjudaAi.ApiService.Options;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace MeAjudaAi.ApiService.Middlewares;

/// <summary>
/// Middleware de Rate Limiting com suporte a usuários autenticados
/// </summary>
public class RateLimitingMiddleware(
    RequestDelegate next,
    IMemoryCache cache,
    RateLimitOptions options,
    ILogger<RateLimitingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = GetClientIpAddress(context);
        var isAuthenticated = context.User.Identity?.IsAuthenticated == true;
        
        var limit = isAuthenticated ? options.Authenticated.RequestsPerMinute : options.Anonymous.RequestsPerMinute;
        
        var key = $"rate_limit:{clientIp}:{context.Request.Path}";
        
        if (!cache.TryGetValue(key, out int requestCount))
        {
            requestCount = 0;
        }

        if (requestCount >= limit)
        {
            logger.LogWarning("Rate limit exceeded for client {ClientIp} on path {Path}. Limit: {Limit}, Current count: {Count}",
                clientIp, context.Request.Path, limit, requestCount);
            await HandleRateLimitExceeded(context, limit);
            return;
        }

        cache.Set(key, requestCount + 1, TimeSpan.FromMinutes(1));
        
        if (requestCount > limit * 0.8) // Log warning when approaching limit (80%)
        {
            logger.LogInformation("Client {ClientIp} approaching rate limit on path {Path}. Current: {Count}/{Limit}",
                clientIp, context.Request.Path, requestCount + 1, limit);
        }
        
        await next(context);
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static async Task HandleRateLimitExceeded(HttpContext context, int limit)
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