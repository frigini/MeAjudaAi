using MeAjudaAi.ApiService.Options;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace MeAjudaAi.ApiService.Middlewares;

/// <summary>
/// Middleware de Rate Limiting com suporte a usuários autenticados
/// </summary>
public class RateLimitingMiddleware(
    RequestDelegate next,
    IMemoryCache cache,
    IOptionsMonitor<RateLimitOptions> options,
    ILogger<RateLimitingMiddleware> logger)
{
    /// <summary>
    /// Simple counter class for rate limiting.
    /// 
    /// <para>
    /// <b>Thread-safety:</b> The <see cref="Value"/> field must only be accessed or modified using thread-safe operations,
    /// such as <see cref="System.Threading.Interlocked.Increment(ref int)"/>. This class is designed to be used in a concurrent environment,
    /// and all modifications to <see cref="Value"/> should be performed atomically.
    /// </para>
    /// </summary>
    private sealed class Counter { public int Value; }
    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = GetClientIpAddress(context);
        var isAuthenticated = context.User.Identity?.IsAuthenticated == true;
        
        var currentOptions = options.CurrentValue;
        var effectiveWindow = TimeSpan.FromSeconds(currentOptions.General.WindowInSeconds);
        
        // Determine effective limit using priority order
        var limit = GetEffectiveLimit(context, currentOptions, isAuthenticated);
        
        var key = $"rate_limit:{clientIp}:{context.Request.Path}";
        
        var counter = cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = effectiveWindow;
            return new Counter();
        }) ?? new Counter();

        var current = Interlocked.Increment(ref counter.Value);

        if (current > limit)
        {
            logger.LogWarning("Rate limit exceeded for client {ClientIp} on path {Path}. Limit: {Limit}, Current count: {Count}, Window: {Window}s",
                clientIp, context.Request.Path, limit, current, currentOptions.General.WindowInSeconds);
            await HandleRateLimitExceeded(context, limit, currentOptions.General.WindowInSeconds);
            return;
        }

        // Counter already incremented; ensure key TTL is set
        cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = effectiveWindow;
            return counter;
        });
        
        if (current >= Math.Floor(limit * 0.8)) // Log warning when approaching limit (80%)
        {
            logger.LogInformation("Client {ClientIp} approaching rate limit on path {Path}. Current: {Count}/{Limit}, Window: {Window}s",
                clientIp, context.Request.Path, current, limit, currentOptions.General.WindowInSeconds);
        }
        
        await next(context);
    }

    private static int GetEffectiveLimit(HttpContext context, RateLimitOptions rateLimitOptions, bool isAuthenticated)
    {
        var requestPath = context.Request.Path.Value ?? string.Empty;
        
        // 1. Check for endpoint-specific limits first
        foreach (var endpointLimit in rateLimitOptions.EndpointLimits)
        {
            if (IsPathMatch(requestPath, endpointLimit.Value.Pattern))
            {
                // Check if this endpoint limit applies to the current user type
                if ((isAuthenticated && endpointLimit.Value.ApplyToAuthenticated) ||
                    (!isAuthenticated && endpointLimit.Value.ApplyToAnonymous))
                {
                    return endpointLimit.Value.RequestsPerMinute;
                }
            }
        }
        
        // 2. Check for role-specific limits (only for authenticated users)
        if (isAuthenticated)
        {
            var userRoles = context.User.FindAll("role")?.Select(c => c.Value) ?? 
                           context.User.FindAll("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Select(c => c.Value) ??
                           [];
            
            foreach (var role in userRoles)
            {
                if (rateLimitOptions.RoleLimits.TryGetValue(role, out var roleLimit))
                {
                    return roleLimit.RequestsPerMinute;
                }
            }
        }
        
        // 3. Fall back to default authenticated/anonymous limits
        return isAuthenticated ? 
            rateLimitOptions.Authenticated.RequestsPerMinute : 
            rateLimitOptions.Anonymous.RequestsPerMinute;
    }
    
    private static bool IsPathMatch(string requestPath, string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            return false;
            
        // Simple wildcard matching - can be enhanced for more complex patterns
        if (pattern.Contains('*'))
        {
            var regexPattern = pattern.Replace("*", ".*");
            return System.Text.RegularExpressions.Regex.IsMatch(requestPath, regexPattern, 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
        
        return string.Equals(requestPath, pattern, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static async Task HandleRateLimitExceeded(HttpContext context, int limit, int windowInSeconds)
    {
        context.Response.StatusCode = 429;
        context.Response.Headers.Append("Retry-After", windowInSeconds.ToString());
        context.Response.ContentType = "application/json";

        var errorResponse = new 
        {
            Error = "RateLimitExceeded",
            Message = "Rate limit exceeded. Please try again later.",
            Details = new Dictionary<string, object>
            {
                ["limit"] = limit,
                ["retryAfterSeconds"] = windowInSeconds,
                ["windowInSeconds"] = windowInSeconds
            }
        };

        var json = JsonSerializer.Serialize(errorResponse, SerializationDefaults.Api);

        await context.Response.WriteAsync(json);
    }
}