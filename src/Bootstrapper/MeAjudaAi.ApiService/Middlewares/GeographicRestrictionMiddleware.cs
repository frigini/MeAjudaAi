using System.Text.Json;
using MeAjudaAi.ApiService.Options;
using MeAjudaAi.Shared.Constants;
using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Shared.Models;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;

namespace MeAjudaAi.ApiService.Middlewares;

/// <summary>
/// Middleware para restringir acesso baseado em localização geográfica.
/// Bloqueia requisições de cidades/estados não permitidos (compliance legal).
/// Usa Microsoft.FeatureManagement para controle dinâmico via Azure App Configuration.
/// </summary>
public class GeographicRestrictionMiddleware(
    RequestDelegate next,
    ILogger<GeographicRestrictionMiddleware> logger,
    IOptionsMonitor<GeographicRestrictionOptions> options,
    IFeatureManager featureManager,
    IGeographicValidationService? geographicValidationService = null)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Verificar se feature está habilitada (Microsoft.FeatureManagement)
        var isFeatureEnabled = await featureManager.IsEnabledAsync(FeatureFlags.GeographicRestriction);

        // Skip se desabilitado via feature flag
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

        // Extrair localização do header X-User-Location ou IP
        var (city, state) = ExtractLocation(context);

        // Validar se cidade/estado está permitido
        var isAllowed = await IsLocationAllowedAsync(city, state, context.RequestAborted);
        if (!isAllowed)
        {
            logger.LogWarning(
                "Geographic restriction: Request blocked from {City}/{State}. IP: {IpAddress}",
                city ?? "Unknown",
                state ?? "Unknown",
                context.Connection.RemoteIpAddress);

            context.Response.StatusCode = 451; // Unavailable For Legal Reasons
            context.Response.ContentType = "application/json";

            var allowedRegions = GetAllowedRegionsDescription();
            var template = options.CurrentValue.BlockedMessage ?? "Access from your region is not allowed. Allowed regions: {allowedRegions}.";
            var message = template.Replace("{allowedRegions}", allowedRegions);

            // Converte entradas configuradas "City|State" (ou nomes simples de cidade) para objetos AllowedCity
            var allowedCitiesResponse = options.CurrentValue.AllowedCities?
                .Select(raw =>
                {
                    var parts = raw.Split('|');
                    var name = parts.Length > 0 ? parts[0].Trim() : raw;
                    var state = parts.Length > 1 ? parts[1].Trim() : string.Empty;

                    return new AllowedCity
                    {
                        Name = name,
                        State = state,
                        IbgeCode = null
                    };
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

        // Localização permitida - continuar pipeline
        await next(context);
    }

    private static (string? City, string? State) ExtractLocation(HttpContext context)
    {
        // Prioridade 1: Header X-User-Location (formato: "City|State")
        if (context.Request.Headers.TryGetValue("X-User-Location", out var locationHeader))
        {
            var parts = locationHeader.ToString().Split('|');
            if (parts.Length == 2)
            {
                var locationCity = parts[0].Trim();
                var locationState = parts[1].Trim();

                // Rejeitar entradas malformadas onde city ou state estão vazios
                // Exemplos rejeitados: "City|", "|State", "City| ", " |State"
                if (!string.IsNullOrEmpty(locationCity) && !string.IsNullOrEmpty(locationState))
                {
                    return (locationCity, locationState);
                }
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

        // TODO: Implementar GeoIP lookup baseado em IP para detectar localização automaticamente.
        //       Opções: MaxMind GeoIP2, IP2Location, ou IPGeolocation API.

        return (null, null);
    }

    private async Task<bool> IsLocationAllowedAsync(string? city, string? state, CancellationToken cancellationToken)
    {
        // Se não conseguiu detectar localização, permitir (fail-open)
        // Produção deve ter GeoIP obrigatório
        if (string.IsNullOrEmpty(city) && string.IsNullOrEmpty(state))
        {
            logger.LogWarning("Geographic restriction: Could not determine user location, allowing access (fail-open)");
            return true;
        }

        // Estratégia 1: Validação simples (case-insensitive string matching)
        // Usada quando IBGE service não está disponível
        var simpleValidation = ValidateLocationSimple(city, state);

        // Estratégia 2: Validação via API IBGE (normalização + verificação precisa)
        // Só executar se o serviço estiver disponível e temos cidade
        if (geographicValidationService is not null && !string.IsNullOrEmpty(city))
        {
            try
            {
                logger.LogDebug("Validating city {City} via IBGE API", city);

                var ibgeValidation = await geographicValidationService.ValidateCityAsync(
                    city,
                    state,
                    cancellationToken);

                // Validação IBGE tem prioridade (mais precisa)
                logger.LogInformation(
                    "IBGE validation for {City}/{State}: {Result} (simple: {SimpleResult})",
                    city, state ?? "N/A", ibgeValidation, simpleValidation);

                return ibgeValidation;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error validating with IBGE, falling back to simple validation");
                // Fallback para validação simples em caso de erro
            }
        }
        else
        {
            if (geographicValidationService is null)
                logger.LogDebug("IBGE validation service not available, using simple validation");
            else if (string.IsNullOrEmpty(city))
                logger.LogDebug("No city provided, skipping IBGE validation");
        }

        // Fallback: validação simples se IBGE falhar ou não estiver disponível
        return simpleValidation;
    }

    private bool ValidateLocationSimple(string? city, string? state)
    {
        // Se temos cidade, validar contra lista de cidades permitidas
        // Suporta tanto formato "City|State" quanto apenas nome da cidade
        if (!string.IsNullOrEmpty(city))
        {
            if (options.CurrentValue.AllowedCities == null)
            {
                logger.LogWarning("Geographic restriction enabled but AllowedCities is null - failing open");
                return true; // Fail-open quando mal configurado
            }

            foreach (var allowedCity in options.CurrentValue.AllowedCities)
            {
                var parts = allowedCity.Split('|');

                if (parts.Length != 2)
                {
                    // Tratar como entrada somente de cidade (sem estado)
                    var configCityOnly = allowedCity.Trim();
                    if (!string.IsNullOrEmpty(configCityOnly) &&
                        configCityOnly.Equals(city, StringComparison.OrdinalIgnoreCase))
                    {
                        // Se não temos estado informado, aceitar apenas pelo nome da cidade
                        if (string.IsNullOrEmpty(state))
                            return true;

                        // Com estado informado, dependerá da regra de estados permitidos
                        return options.CurrentValue.AllowedStates?.Any(s =>
                            s.Equals(state, StringComparison.OrdinalIgnoreCase)) == true;
                    }
                    continue;
                }

                var configCity = parts[0].Trim();
                var configState = parts[1].Trim();

                // Rejeitar entradas onde city ou state estão vazios
                if (string.IsNullOrEmpty(configCity) || string.IsNullOrEmpty(configState))
                {
                    logger.LogWarning("Malformed configuration (empty values): {AllowedCity}", allowedCity);
                    continue;
                }

                // Match city (case-insensitive)
                if (configCity.Equals(city, StringComparison.OrdinalIgnoreCase))
                {
                    // Se temos state, validar também
                    if (!string.IsNullOrEmpty(state))
                    {
                        return configState.Equals(state, StringComparison.OrdinalIgnoreCase);
                    }
                    // Se não temos state, match apenas por cidade
                    return true;
                }
            }
            return false;
        }

        // Se temos apenas estado (sem cidade), validar contra lista de estados permitidos
        if (!string.IsNullOrEmpty(state))
        {
            if (options.CurrentValue.AllowedStates == null)
            {
                logger.LogWarning("Geographic restriction enabled but AllowedStates is null - failing open");
                return true; // Fail-open quando mal configurado
            }

            return options.CurrentValue.AllowedStates.Any(s => s.Equals(state, StringComparison.OrdinalIgnoreCase));
        }

        return false;
    }

    private string GetAllowedRegionsDescription()
    {
        var cities = options.CurrentValue.AllowedCities?.Any() == true
            ? string.Join(", ", options.CurrentValue.AllowedCities)
            : "N/A";

        var states = options.CurrentValue.AllowedStates?.Any() == true
            ? string.Join(", ", options.CurrentValue.AllowedStates)
            : "N/A";

        return $"Cities: {cities} | States: {states}";
    }
}
