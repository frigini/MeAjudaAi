using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using MeAjudaAi.Shared.Configuration;

namespace MeAjudaAi.Shared.Middleware;

/// <summary>
/// Middleware para restringir acesso baseado em localização geográfica.
/// Bloqueia requisições de cidades/estados não permitidos (compliance legal).
/// </summary>
public class GeographicRestrictionMiddleware(
    RequestDelegate next,
    ILogger<GeographicRestrictionMiddleware> logger,
    IOptions<GeographicRestrictionOptions> options)
{
    private readonly GeographicRestrictionOptions _options = options.Value;

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip se desabilitado (ex: Development)
        if (!_options.Enabled)
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

        // Extrair localização do header X-User-Location ou IP
        var (city, state) = ExtractLocation(context);

        // Validar se cidade/estado está permitido
        if (!IsLocationAllowed(city, state))
        {
            logger.LogWarning(
                "Geographic restriction: Request blocked from {City}/{State}. IP: {IpAddress}",
                city ?? "Unknown",
                state ?? "Unknown",
                context.Connection.RemoteIpAddress);

            context.Response.StatusCode = 451; // Unavailable For Legal Reasons
            context.Response.ContentType = "application/json";

            var allowedRegions = GetAllowedRegionsDescription();
            var errorResponse = new
            {
                error = "geographic_restriction",
                message = string.Format(_options.BlockedMessage, allowedRegions),
                allowedCities = _options.AllowedCities,
                allowedStates = _options.AllowedStates,
                yourLocation = new { city, state }
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                }));

            return;
        }

        // Localização permitida - continuar pipeline
        await next(context);
    }

    private (string? City, string? State) ExtractLocation(HttpContext context)
    {
        // Prioridade 1: Header X-User-Location (formato: "City|State")
        if (context.Request.Headers.TryGetValue("X-User-Location", out var locationHeader))
        {
            var parts = locationHeader.ToString().Split('|');
            if (parts.Length == 2)
            {
                return (parts[0].Trim(), parts[1].Trim());
            }
        }

        // Prioridade 2: Header X-User-City e X-User-State (separados)
        var city = context.Request.Headers.TryGetValue("X-User-City", out var cityHeader)
            ? cityHeader.ToString().Trim()
            : null;

        var state = context.Request.Headers.TryGetValue("X-User-State", out var stateHeader)
            ? stateHeader.ToString().Trim()
            : null;

        if (!string.IsNullOrEmpty(city) || !string.IsNullOrEmpty(state))
        {
            return (city, state);
        }

        // TODO Sprint 2: Implementar GeoIP lookup baseado em IP
        // var ip = context.Connection.RemoteIpAddress;
        // return await _geoIpService.GetLocationFromIpAsync(ip);

        return (null, null);
    }

    private bool IsLocationAllowed(string? city, string? state)
    {
        // Se não conseguiu detectar localização, permitir (fail-open)
        // Produção deve ter GeoIP obrigatório
        if (string.IsNullOrEmpty(city) && string.IsNullOrEmpty(state))
        {
            logger.LogWarning("Geographic restriction: Could not determine user location, allowing access (fail-open)");
            return true;
        }

        // Validar cidade (case-insensitive)
        if (!string.IsNullOrEmpty(city) &&
            _options.AllowedCities.Any(c => c.Equals(city, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        // Validar estado (case-insensitive, sigla de 2 letras)
        if (!string.IsNullOrEmpty(state) &&
            _options.AllowedStates.Any(s => s.Equals(state, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return false;
    }

    private string GetAllowedRegionsDescription()
    {
        var cities = _options.AllowedCities.Any()
            ? string.Join(", ", _options.AllowedCities)
            : "N/A";

        var states = _options.AllowedStates.Any()
            ? string.Join(", ", _options.AllowedStates)
            : "N/A";

        return $"Cidades: {cities} | Estados: {states}";
    }
}
