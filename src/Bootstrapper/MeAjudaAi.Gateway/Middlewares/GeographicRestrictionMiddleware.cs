using System.Text.Json;
using MeAjudaAi.Gateway.Options;
using MeAjudaAi.Shared.Utilities.Constants;
using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Contracts.Models;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;

namespace MeAjudaAi.Gateway.Middlewares;

/// <summary>
/// Middleware para restringir acesso baseado em localização geográfica.
/// Bloqueia requisições de cidades/estados não permitidos (compliance legal).
/// </summary>
public class GeographicRestrictionMiddleware(
    RequestDelegate next,
    ILogger<GeographicRestrictionMiddleware> logger,
    IOptionsMonitor<GeographicRestrictionOptions> options,
    IFeatureManager featureManager)
{
    public async Task InvokeAsync(HttpContext context, IGeographicValidationService? geographicValidationService = null)
    {
        // Verificar se feature está habilitada
        var isFeatureEnabled = await featureManager.IsEnabledAsync(FeatureFlags.GeographicRestriction);

        if (!isFeatureEnabled)
        {
            await next(context);
            return;
        }

        // Skip health checks e endpoints internos
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

            var allowedCitiesResponse = options.CurrentValue.AllowedCities?
                .Select(raw =>
                {
                    var parts = raw.Split('|');
                    var name = parts.Length > 0 ? parts[0].Trim() : raw;
                    var state = parts.Length > 1 ? parts[1].Trim() : string.Empty;

                    return new AllowedCity { Name = name, State = state };
                });

            var errorResponse = new GeographicRestrictionErrorResponse(
                message: message,
                userLocation: new UserLocation { City = city, State = state },
                allowedCities: allowedCitiesResponse,
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
        if (!string.IsNullOrEmpty(city))
        {
            if (options.CurrentValue.AllowedCities == null) return true;

            foreach (var allowedCity in options.CurrentValue.AllowedCities)
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
                        if (options.CurrentValue.AllowedStates == null || 
                            !options.CurrentValue.AllowedStates.Any())
                        {
                            return false;
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
                    return true;
                }
            }
            return false;
        }

        if (!string.IsNullOrEmpty(state))
        {
            if (options.CurrentValue.AllowedStates == null) return true;
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
