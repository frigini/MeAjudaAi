using System.Text.Json;
using MeAjudaAi.ApiService.Options;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

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
    /// Classe contador simples para rate limiting.
    /// <para>
    /// <b>Thread-safety:</b> O campo <see cref="Value"/> deve ser acessado ou modificado apenas usando operações thread-safe,
    /// como <see cref="System.Threading.Interlocked.Increment(ref int)"/>. Esta classe foi projetada para ser usada em um ambiente concorrente,
    /// e todas as modificações no <see cref="Value"/> devem ser realizadas atomicamente.
    /// </para>
    /// </summary>
    private sealed class Counter
    {
        public int Value;
        public DateTime ExpiresAt;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        var currentOptions = options.CurrentValue;

        // Bypass rate limiting if explicitly disabled
        if (!currentOptions.General.Enabled)
        {
            await next(context);
            return;
        }

        var clientIp = GetClientIpAddress(context);
        var isAuthenticated = context.User.Identity?.IsAuthenticated == true;

        // Check IP whitelist first - bypass rate limiting if IP is whitelisted
        if (currentOptions.General.EnableIpWhitelist &&
            currentOptions.General.WhitelistedIps.Contains(clientIp))
        {
            await next(context);
            return;
        }

        // Defensively clamp window to at least 1 second
        var windowSeconds = Math.Max(1, currentOptions.General.WindowInSeconds);
        var effectiveWindow = TimeSpan.FromSeconds(windowSeconds);

        // Determine effective limit using priority order
        var limit = GetEffectiveLimit(context, currentOptions, isAuthenticated, effectiveWindow);

        // Key by user (when authenticated) and method to reduce false sharing
        var userKey = isAuthenticated
            ? (context.User.FindFirst("sub")?.Value ?? context.User.Identity?.Name ?? clientIp)
            : clientIp;
        var key = $"rate_limit:{userKey}:{context.Request.Method}:{context.Request.Path}";

        var counter = cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = effectiveWindow;
            return new Counter { ExpiresAt = DateTime.UtcNow + effectiveWindow };
        })!; // GetOrCreate never returns null when factory returns a value

        var current = Interlocked.Increment(ref counter.Value);

        if (current > limit)
        {
            logger.LogWarning("Rate limit exceeded for client {ClientIp} on path {Path}. Limit: {Limit}, Current count: {Count}, Window: {Window}s",
                clientIp, context.Request.Path, limit, current, windowSeconds);
            await HandleRateLimitExceeded(context, counter, currentOptions.General.ErrorMessage, (int)effectiveWindow.TotalSeconds);
            return;
        }

        // TTL set at creation; no need for redundant cache operation

        var warnThreshold = (int)Math.Ceiling(limit * 0.8);
        if (current >= warnThreshold) // approaching limit (80%)
        {
            logger.LogInformation("Client {ClientIp} approaching rate limit on path {Path}. Current: {Count}/{Limit}, Window: {Window}s",
                clientIp, context.Request.Path, current, limit, currentOptions.General.WindowInSeconds);
        }

        await next(context);
    }

    private static int GetEffectiveLimit(HttpContext context, RateLimitOptions rateLimitOptions, bool isAuthenticated, TimeSpan window)
    {
        var requestPath = context.Request.Path.Value ?? string.Empty;

        // 1. Check for endpoint-specific limits first
        var matchingLimit = rateLimitOptions.EndpointLimits
            .Where(endpointLimit => IsPathMatch(requestPath, endpointLimit.Value.Pattern))
            .FirstOrDefault(endpointLimit =>
                (isAuthenticated && endpointLimit.Value.ApplyToAuthenticated) ||
                (!isAuthenticated && endpointLimit.Value.ApplyToAnonymous));

        if (matchingLimit.Value != null)
        {
            return ScaleToWindow(
                matchingLimit.Value.RequestsPerMinute,
                matchingLimit.Value.RequestsPerHour,
                0,
                window);
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
                    return ScaleToWindow(
                        roleLimit.RequestsPerMinute,
                        roleLimit.RequestsPerHour,
                        roleLimit.RequestsPerDay,
                        window);
                }
            }
        }

        // 3. Fall back to default authenticated/anonymous limits
        return isAuthenticated
            ? ScaleToWindow(rateLimitOptions.Authenticated.RequestsPerMinute, rateLimitOptions.Authenticated.RequestsPerHour, rateLimitOptions.Authenticated.RequestsPerDay, window)
            : ScaleToWindow(rateLimitOptions.Anonymous.RequestsPerMinute, rateLimitOptions.Anonymous.RequestsPerHour, rateLimitOptions.Anonymous.RequestsPerDay, window);
    }

    private static int ScaleToWindow(int perMinute, int perHour, int perDay, TimeSpan window)
    {
        var secs = Math.Max(1, (int)window.TotalSeconds);
        var candidates = new List<double>(3);
        if (perMinute > 0) candidates.Add(perMinute * secs / 60.0);
        if (perHour > 0) candidates.Add(perHour * secs / 3600.0);
        if (perDay > 0) candidates.Add(perDay * secs / 86400.0);
        var allowed = candidates.Count > 0 ? candidates.Min() : 0.0;
        return Math.Max(1, (int)Math.Floor(allowed));
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

    private static async Task HandleRateLimitExceeded(HttpContext context, Counter counter, string errorMessage, int windowInSeconds)
    {
        // Calculate remaining TTL from counter expiration
        var retryAfterSeconds = Math.Max(0, (int)Math.Ceiling((counter.ExpiresAt - DateTime.UtcNow).TotalSeconds));

        context.Response.StatusCode = 429;
        context.Response.Headers.Append("Retry-After", retryAfterSeconds.ToString());
        context.Response.ContentType = "application/json";

        var errorResponse = new
        {
            Error = "RateLimitExceeded",
            Message = errorMessage,
            Details = new Dictionary<string, object>
            {
                ["retryAfterSeconds"] = retryAfterSeconds,
                ["windowInSeconds"] = windowInSeconds
            }
        };

        var json = JsonSerializer.Serialize(errorResponse, SerializationDefaults.Api);

        await context.Response.WriteAsync(json);
    }
}
