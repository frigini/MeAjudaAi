using System.Text.Json;
using MeAjudaAi.Shared.Utilities.Constants;
using MeAjudaAi.Shared.Geolocation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;

namespace MeAjudaAi.Shared.Middleware;

public class GeographicRestrictionMiddleware(
    RequestDelegate next,
    ILogger<GeographicRestrictionMiddleware> logger,
    IOptionsMonitor<GeographicRestrictionOptions> options,
    IFeatureManager featureManager)
{
    public async Task InvokeAsync(HttpContext context, IGeographicValidationService? geographicValidationService = null)
    {
        var isFeatureEnabled = await featureManager.IsEnabledAsync(FeatureFlags.GeographicRestriction);

        if (!isFeatureEnabled)
        {
            await next(context);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/health", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/_framework", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        var (city, state) = ExtractLocation(context);

        var isAllowed = await IsLocationAllowedAsync(city, state, geographicValidationService, context.RequestAborted);
        if (!isAllowed)
        {
            logger.LogWarning(
                "Geographic restriction: Request blocked from {City}/{State}. IP: {IpAddress}",
                city ?? "Unknown",
                state ?? "Unknown",
                context.Connection.RemoteIpAddress);

            context.Response.StatusCode = 451;
            context.Response.ContentType = "application/json";

            var allowedRegions = GetAllowedRegionsDescription();
            var template = options.CurrentValue.BlockedMessage ?? "Acesso da sua região não permitido. Regiões permitidas: {allowedRegions}.";
            var message = template.Replace("{allowedRegions}", allowedRegions);

            var errorResponse = new GeographicRestrictionErrorResponse(
                message: message,
                userLocation: UserLocation.Create(city, state),
                allowedCities: options.CurrentValue.AllowedCities?.Select((name, index) => AllowedCity.Create(name, null)),
                allowedStates: options.CurrentValue.AllowedStates);

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                }));

            return;
        }

        await next(context);
    }

    private static (string? City, string? State) ExtractLocation(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-User-Location", out var locationHeader))
        {
            var headerSpan = locationHeader.ToString().AsSpan();
            var separatorIndex = headerSpan.IndexOf('|');
            
            if (separatorIndex >= 0)
            {
                var remainder = headerSpan[(separatorIndex + 1)..];
                if (remainder.IndexOf('|') >= 0) return (string.Empty, string.Empty);

                var locationCity = headerSpan[..separatorIndex].Trim().ToString();
                var locationState = remainder.Trim().ToString();

                if (!string.IsNullOrWhiteSpace(locationCity) && !string.IsNullOrWhiteSpace(locationState))
                {
                    return (locationCity, locationState);
                }
            }
            return (string.Empty, string.Empty);
        }

        var cityPresent = context.Request.Headers.TryGetValue("X-User-City", out var cityHeader);
        var statePresent = context.Request.Headers.TryGetValue("X-User-State", out var stateHeader);

        if (cityPresent || statePresent)
        {
            var city = cityPresent ? (string.IsNullOrWhiteSpace(cityHeader.ToString()) ? string.Empty : cityHeader.ToString().Trim()) : null;
            var state = statePresent ? (string.IsNullOrWhiteSpace(stateHeader.ToString()) ? string.Empty : stateHeader.ToString().Trim()) : null;
            return (city, state);
        }

        return (null, null);
    }

    private async Task<bool> IsLocationAllowedAsync(string? city, string? state, IGeographicValidationService? geographicValidationService, CancellationToken cancellationToken)
    {
        if (city?.Length == 0 || state?.Length == 0)
        {
            logger.LogWarning("Geographic restriction: Malformed or empty location header detected, rejecting request.");
            return false;
        }

        if (string.IsNullOrEmpty(city) && string.IsNullOrEmpty(state))
        {
            var failOpen = options.CurrentValue.FailOpen;
            if (failOpen)
            {
                logger.LogWarning("Geographic restriction: Could not determine user location, allowing access (FailOpen=true)");
                return true;
            }
            logger.LogError("Geographic restriction: Could not determine user location, rejecting request (FailOpen=false)");
            return false;
        }

        var simpleValidation = ValidateLocationSimple(city, state);

        if (geographicValidationService is not null && !string.IsNullOrEmpty(city))
        {
            try
            {
                var ibgeValidation = await geographicValidationService.ValidateCityAsync(city, state, cancellationToken);
                return ibgeValidation;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error validating with IBGE, falling back to simple validation");
            }
        }

        return simpleValidation;
    }

    private bool ValidateLocationSimple(string? city, string? state)
    {
        var hasAllowedCities = options.CurrentValue.AllowedCities?.Any() ?? false;
        var hasAllowedStates = options.CurrentValue.AllowedStates?.Any() ?? false;

        if (!hasAllowedCities && !hasAllowedStates)
        {
            return true;
        }

        if (!string.IsNullOrEmpty(city))
        {
            if (!hasAllowedCities && !hasAllowedStates) return false;

            if (!hasAllowedCities)
            {
                if (hasAllowedStates && !string.IsNullOrEmpty(state) &&
                    options.CurrentValue.AllowedStates.Any(s => s.Equals(state, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
                return false;
            }

            foreach (var allowedCity in options.CurrentValue.AllowedCities ?? [])
            {
                var citySpan = allowedCity.AsSpan();
                var separatorIndex = citySpan.IndexOf('|');

                if (separatorIndex < 0)
                {
                    var configCityOnly = citySpan.Trim().ToString();
                    if (!string.IsNullOrEmpty(configCityOnly) &&
                        configCityOnly.Equals(city, StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.IsNullOrEmpty(state)) return true;
                        if (!options.CurrentValue.AllowedStates.Any())
                        {
                            return true;
                        }
                        return options.CurrentValue.AllowedStates.Any(s =>
                            s.Equals(state, StringComparison.OrdinalIgnoreCase));
                    }
                    continue;
                }

                var configCity = citySpan[..separatorIndex].Trim().ToString();
                var configState = citySpan[(separatorIndex + 1)..].Trim().ToString();

                if (configCity.Equals(city, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(configState))
                    {
                        return true;
                    }
                    if (!string.IsNullOrEmpty(state))
                    {
                        if (configState.Equals(state, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                        continue;
                    }
                    continue;
                }
            }
            return false;
        }

        if (!string.IsNullOrEmpty(state))
        {
            if (!hasAllowedStates) return false;
            return options.CurrentValue.AllowedStates.Any(s => s.Equals(state, StringComparison.OrdinalIgnoreCase));
        }

        return false;
    }

    private string GetAllowedRegionsDescription()
    {
        var cities = options.CurrentValue.AllowedCities?.Any() == true ? string.Join(", ", options.CurrentValue.AllowedCities) : "Nenhuma";
        var states = options.CurrentValue.AllowedStates?.Any() == true ? string.Join(", ", options.CurrentValue.AllowedStates) : "Nenhum";
        return $"Cidades: {cities} | Estados: {states}";
    }
}

public class GeographicRestrictionOptions
{
    public const string SectionName = "GeographicRestriction";

    public bool Enabled { get; set; } = false;
    public List<string> AllowedStates { get; set; } = [];
    public List<string> AllowedCities { get; set; } = [];
    public string? BlockedMessage { get; set; }
    public string? DefaultBlockedMessage { get; set; }
    public bool FailOpen { get; set; } = true;
}

public record GeographicRestrictionErrorResponse(
    string message,
    UserLocation? userLocation,
    IEnumerable<AllowedCity>? allowedCities,
    List<string>? allowedStates);

public record AllowedCity(string Name, string? State)
{
    public static AllowedCity Create(string name, string? state = null) => new(name, state);
}

public record UserLocation(string? City, string? State)
{
    public static UserLocation Create(string? city, string? state) => new(city, state);
}