using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.Shared.Middleware;

public class RateLimitingMiddleware(
    RequestDelegate next,
    ILogger<RateLimitingMiddleware> logger,
    IOptionsMonitor<RateLimitingOptions> options,
    IMemoryCache cache)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var currentOptions = options.CurrentValue;
        if (!currentOptions.General.Enabled)
        {
            await next(context);
            return;
        }

        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        if (currentOptions.General.EnableIpWhitelist && currentOptions.General.WhitelistedIps.Contains(clientIp))
        {
            await next(context);
            return;
        }

        var isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;
        int requestsPerMinute, requestsPerHour, requestsPerDay;

        if (isAuthenticated)
        {
            requestsPerMinute = currentOptions.Authenticated.RequestsPerMinute;
            requestsPerHour = currentOptions.Authenticated.RequestsPerHour;
            requestsPerDay = currentOptions.Authenticated.RequestsPerDay;
        }
        else
        {
            requestsPerMinute = currentOptions.Anonymous.RequestsPerMinute;
            requestsPerHour = currentOptions.Anonymous.RequestsPerHour;
            requestsPerDay = currentOptions.Anonymous.RequestsPerDay;
        }

        var windowSeconds = Math.Max(1, currentOptions.General.WindowInSeconds);
        var window = TimeSpan.FromSeconds(windowSeconds);

        var windowKey = $"rate_limit_{clientIp}_{isAuthenticated}";
        var counter = cache.GetOrCreate(windowKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = window;
            return new RateLimitCounter();
        });

        var currentCount = counter.IncrementAndGet();

        var scaledLimit = CalculateScaledLimit(requestsPerMinute, requestsPerHour, requestsPerDay, windowSeconds);
        if (currentCount > scaledLimit)
        {
            logger.LogWarning(
                "Rate limit exceeded for {ClientIp} (Authenticated: {IsAuthenticated}). Count: {Count}, Limit: {Limit}",
                clientIp, isAuthenticated, currentCount, scaledLimit);

            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";
            context.Response.Headers.Append("Retry-After", windowSeconds.ToString());

            var errorResponse = new
            {
                error = "RateLimitExceeded",
                message = currentOptions.General.ErrorMessage,
                retryAfterSeconds = windowSeconds
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
            return;
        }

        await next(context);
    }

    private static int CalculateScaledLimit(int perMinute, int perHour, int perDay, int windowSeconds)
    {
        var candidates = new List<double>();
        if (perMinute > 0) candidates.Add(perMinute * windowSeconds / 60.0);
        if (perHour > 0) candidates.Add(perHour * windowSeconds / 3600.0);
        if (perDay > 0) candidates.Add(perDay * windowSeconds / 86400.0);

        return candidates.Count > 0 ? Math.Max(1, (int)Math.Floor(candidates.Min())) : 1;
    }
}

public class RateLimitCounter
{
    private int _value;

    public int Value => _value;

    public int IncrementAndGet() => Interlocked.Increment(ref _value);
}

public class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public GeneralSettings General { get; set; } = new();
    public AnonymousLimits Anonymous { get; set; } = new();
    public AuthenticatedLimits Authenticated { get; set; } = new();
}

public class GeneralSettings
{
    public bool Enabled { get; set; } = true;
    public int WindowInSeconds { get; set; } = 60;
    public bool EnableIpWhitelist { get; set; } = false;
    public List<string> WhitelistedIps { get; set; } = [];
    public string ErrorMessage { get; set; } = "Limite de requisições excedido. Tente novamente mais tarde.";
}

public class AnonymousLimits
{
    public int RequestsPerMinute { get; set; } = 30;
    public int RequestsPerHour { get; set; } = 300;
    public int RequestsPerDay { get; set; } = 1000;
}

public class AuthenticatedLimits
{
    public int RequestsPerMinute { get; set; } = 120;
    public int RequestsPerHour { get; set; } = 2000;
    public int RequestsPerDay { get; set; } = 10000;
}