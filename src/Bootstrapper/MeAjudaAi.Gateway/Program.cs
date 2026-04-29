using System.Text.Json;
using MeAjudaAi.Gateway.Middlewares;
using MeAjudaAi.Gateway.Options;
using MeAjudaAi.Modules.Locations.Infrastructure.Services;
using MeAjudaAi.ServiceDefaults;
using MeAjudaAi.Shared.Geolocation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.FeatureManagement;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.Configure<GatewayCorsOptions>(
    builder.Configuration.GetSection(GatewayCorsOptions.SectionName));
builder.Services.Configure<GeographicRestrictionOptions>(
    builder.Configuration.GetSection(GeographicRestrictionOptions.SectionName));
builder.Services.Configure<GatewayRateLimitOptions>(
    builder.Configuration.GetSection(GatewayRateLimitOptions.SectionName));

builder.Services.AddHttpClient<IGeographicValidationService, GeographicValidationService>();
builder.Services.AddMemoryCache();
builder.Services.AddFeatureManagement();

var corsConfig = builder.Configuration.GetSection(GatewayCorsOptions.SectionName).Get<GatewayCorsOptions>() ?? new GatewayCorsOptions();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsConfig.AllowedOrigins.ToArray())
              .WithMethods(corsConfig.AllowedMethods.ToArray())
              .WithHeaders(corsConfig.AllowedHeaders.ToArray())
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromSeconds(corsConfig.MaxAgeSeconds));
    });
});

var keycloakBaseUrl = builder.Configuration["Keycloak:BaseUrl"] ?? "http://localhost:8080";
var keycloakRealm = builder.Configuration["Keycloak:Realm"] ?? "meajudaai";
var keycloakAudience = builder.Configuration["Keycloak:Audience"] ?? "account";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"{keycloakBaseUrl}/realms/{keycloakRealm}";
        options.Audience = keycloakAudience;
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

builder.Services.AddAuthorization();

var timeoutSeconds = builder.Configuration.GetValue<int>("GatewayResilience:TimeoutSeconds", 30);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(transforms =>
    {
        transforms.AddRequestTransform(async context =>
        {
            context.ProxyRequest.Headers.Add("X-Forwarded-For", context.HttpContext.Connection.RemoteIpAddress?.ToString());
            context.ProxyRequest.Headers.Add("X-Forwarded-Proto", "https");
            context.ProxyRequest.Headers.Add("X-Original-Host", context.HttpContext.Request.Host.ToString());
            context.ProxyRequest.Headers.Add("X-Gateway-Name", "MeAjudaAi-Gateway");
            context.ProxyRequest.Headers.Add("X-Request-Timeout", timeoutSeconds.ToString());
        });
    });

var app = builder.Build();

app.UseCors();

app.Use(async (context, next) =>
{
    var rateLimitOptions = context.RequestServices.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<GatewayRateLimitOptions>>();
    var options = rateLimitOptions.CurrentValue;
    if (!options.General.Enabled)
    {
        await next();
        return;
    }

    var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    if (options.General.EnableIpWhitelist && options.General.WhitelistedIps.Contains(clientIp))
    {
        await next();
        return;
    }

    var isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;
    int requestsPerMinute, requestsPerHour, requestsPerDay;

    if (isAuthenticated)
    {
        requestsPerMinute = options.Authenticated.RequestsPerMinute;
        requestsPerHour = options.Authenticated.RequestsPerHour;
        requestsPerDay = options.Authenticated.RequestsPerDay;
    }
    else
    {
        requestsPerMinute = options.Anonymous.RequestsPerMinute;
        requestsPerHour = options.Anonymous.RequestsPerHour;
        requestsPerDay = options.Anonymous.RequestsPerDay;
    }

    var windowSeconds = Math.Max(1, options.General.WindowInSeconds);
    var window = TimeSpan.FromSeconds(windowSeconds);

    var windowKey = $"gateway_rate_{clientIp}_{isAuthenticated}";
    var cache = context.RequestServices.GetRequiredService<IMemoryCache>();
    var counter = cache.GetOrCreate(windowKey, entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = window;
        return new GatewayRateCounter();
    });

    counter.Value = counter.Value + 1;

    var scaledLimit = CalculateScaledLimit(requestsPerMinute, requestsPerHour, requestsPerDay, windowSeconds);
    if (counter.Value > scaledLimit)
    {
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Response.ContentType = "application/json";
        context.Response.Headers.Append("Retry-After", windowSeconds.ToString());
        var errorResponse = new
        {
            error = "RateLimitExceeded",
            message = options.General.ErrorMessage,
            retryAfterSeconds = windowSeconds
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        return;
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();

app.UseMiddleware<GeographicRestrictionMiddleware>();

app.MapReverseProxy();

app.Run();

static int CalculateScaledLimit(int perMinute, int perHour, int perDay, int windowSeconds)
{
    var candidates = new List<double>();
    if (perMinute > 0) candidates.Add(perMinute * windowSeconds / 60.0);
    if (perHour > 0) candidates.Add(perHour * windowSeconds / 3600.0);
    if (perDay > 0) candidates.Add(perDay * windowSeconds / 86400.0);

    return candidates.Count > 0 ? Math.Max(1, (int)Math.Floor(candidates.Min())) : 1;
}

class GatewayRateCounter
{
    public int Value { get; set; }
}