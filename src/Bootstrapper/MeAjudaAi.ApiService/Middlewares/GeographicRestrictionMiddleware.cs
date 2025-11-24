using System.Text.Json;
using MeAjudaAi.ApiService.Options;
using MeAjudaAi.Shared.Constants;
using MeAjudaAi.Shared.Geolocation;
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
    IOptions<GeographicRestrictionOptions> options,
    IFeatureManager featureManager,
    IGeographicValidationService? geographicValidationService = null)
{
    private readonly GeographicRestrictionOptions _options = options.Value;

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
            var message = _options.BlockedMessage.Replace("{allowedRegions}", allowedRegions);

            var errorResponse = new
            {
                error = "geographic_restriction",
                message,
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

        // TODO Sprint 2: Implementar GeoIP lookup baseado em IP
        // Opção 1: GeoIP (MaxMind, IP2Location)
        // var ip = context.Connection.RemoteIpAddress;
        // return await _geoIpService.GetLocationFromIpAsync(ip);

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
                logger.LogDebug("Validando cidade {City} via API IBGE", city);

                var ibgeValidation = await geographicValidationService.ValidateCityAsync(
                    city,
                    state,
                    _options.AllowedCities,
                    cancellationToken);

                // IBGE validation tem prioridade (mais precisa)
                logger.LogInformation(
                    "Validação IBGE para {City}/{State}: {Result} (simples: {SimpleResult})",
                    city, state ?? "N/A", ibgeValidation, simpleValidation);

                return ibgeValidation;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao validar com IBGE, fallback para validação simples");
                // Fallback para validação simples em caso de erro
            }
        }

        // Fallback: validação simples se IBGE falhar ou não estiver disponível
        return simpleValidation;
    }

    private bool ValidateLocationSimple(string? city, string? state)
    {
        // Se temos cidade, validar contra lista de cidades permitidas
        if (!string.IsNullOrEmpty(city))
        {
            // Validar contra formato "City|State" das cidades permitidas
            foreach (var allowedCity in _options.AllowedCities)
            {
                var parts = allowedCity.Split('|');
                if (parts.Length != 2)
                {
                    // Rejeitar configuração malformada
                    logger.LogWarning("Configuração malformada de cidade permitida: {AllowedCity}", allowedCity);
                    continue;
                }

                var configCity = parts[0].Trim();
                var configState = parts[1].Trim();

                // Rejeitar entradas onde city ou state estão vazios
                if (string.IsNullOrEmpty(configCity) || string.IsNullOrEmpty(configState))
                {
                    logger.LogWarning("Configuração malformada (valores vazios): {AllowedCity}", allowedCity);
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
            return _options.AllowedStates.Any(s => s.Equals(state, StringComparison.OrdinalIgnoreCase));
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
