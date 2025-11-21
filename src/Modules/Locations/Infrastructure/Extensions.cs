using MeAjudaAi.Modules.Locations.Application.ModuleApi;
using MeAjudaAi.Modules.Locations.Application.Services;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients;
using MeAjudaAi.Modules.Locations.Infrastructure.Services;
using MeAjudaAi.Shared.Contracts.Modules.Location;
using MeAjudaAi.Shared.Geolocation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Locations.Infrastructure;

/// <summary>
/// Métodos de extensão para registrar serviços do módulo Location.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Registra todos os serviços do módulo Location.
    /// </summary>
    public static IServiceCollection AddLocationModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Registrar HTTP clients para APIs de CEP
        // ServiceDefaults já configura resiliência (retry, circuit breaker, timeout)
        services.AddHttpClient<ViaCepClient>(client =>
        {
            var baseUrl = configuration["Locations:ExternalApis:ViaCep:BaseUrl"]
                ?? throw new InvalidOperationException("Locations:ExternalApis:ViaCep:BaseUrl não configurado");
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<BrasilApiCepClient>(client =>
        {
            var baseUrl = configuration["Locations:ExternalApis:BrasilApi:BaseUrl"]
                ?? throw new InvalidOperationException("Locations:ExternalApis:BrasilApi:BaseUrl não configurado");
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<OpenCepClient>(client =>
        {
            var baseUrl = configuration["Locations:ExternalApis:OpenCep:BaseUrl"]
                ?? throw new InvalidOperationException("Locations:ExternalApis:OpenCep:BaseUrl não configurado");
            client.BaseAddress = new Uri(baseUrl);
        });

        // Registrar HTTP client para Nominatim (geocoding)
        services.AddHttpClient<NominatimClient>(client =>
        {
            var baseUrl = configuration["Locations:ExternalApis:Nominatim:BaseUrl"]
                ?? throw new InvalidOperationException("Locations:ExternalApis:Nominatim:BaseUrl não configurado");
            client.BaseAddress = new Uri(baseUrl);

            // Configurar User-Agent conforme política de uso do Nominatim
            var userAgent = configuration["Locations:ExternalApis:Nominatim:UserAgent"]
                ?? throw new InvalidOperationException("Locations:ExternalApis:Nominatim:UserAgent não configurado");
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        });

        // Registrar HTTP client para IBGE Localidades
        services.AddHttpClient<IIbgeClient, IbgeClient>(client =>
        {
            var baseUrl = configuration["Locations:ExternalApis:IBGE:BaseUrl"]
                ?? throw new InvalidOperationException("Locations:ExternalApis:IBGE:BaseUrl não configurado");
            client.BaseAddress = new Uri(baseUrl);
        });

        // Registrar serviços
        services.AddScoped<ICepLookupService, CepLookupService>();
        services.AddScoped<IGeocodingService, GeocodingService>();
        services.AddScoped<IIbgeService, IbgeService>();

        // Registrar adapter para middleware (Shared → Locations)
        services.AddScoped<IGeographicValidationService, GeographicValidationService>();

        // Registrar Module API
        services.AddScoped<ILocationModuleApi, LocationsModuleApi>();

        return services;
    }

    /// <summary>
    /// Configura o middleware do módulo Location.
    /// Location module exposes only internal services, no endpoints or middleware.
    /// This method exists for consistency with other modules.
    /// </summary>
    public static WebApplication UseLocationModule(this WebApplication app)
    {
        // No middleware or endpoints to configure
        return app;
    }
}
