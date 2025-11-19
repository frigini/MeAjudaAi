using MeAjudaAi.Modules.Locations.Application.ModuleApi;
using MeAjudaAi.Modules.Locations.Application.Services;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients;
using MeAjudaAi.Modules.Locations.Infrastructure.Services;
using MeAjudaAi.Shared.Contracts.Modules.Location;
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
            var baseUrl = configuration["Locations:ExternalApis:ViaCep:BaseUrl"] ?? "https://viacep.com.br/";
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<BrasilApiCepClient>(client =>
        {
            var baseUrl = configuration["Locations:ExternalApis:BrasilApi:BaseUrl"] ?? "https://brasilapi.com.br/";
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<OpenCepClient>(client =>
        {
            var baseUrl = configuration["Locations:ExternalApis:OpenCep:BaseUrl"] ?? "https://opencep.com/";
            client.BaseAddress = new Uri(baseUrl);
        });

        // Registrar HTTP client para Nominatim (geocoding)
        services.AddHttpClient<NominatimClient>(client =>
        {
            var baseUrl = configuration["Locations:ExternalApis:Nominatim:BaseUrl"] ?? "https://nominatim.openstreetmap.org/";
            client.BaseAddress = new Uri(baseUrl);

            // Configurar User-Agent conforme política de uso do Nominatim
            var userAgent = configuration["Locations:ExternalApis:Nominatim:UserAgent"]
                ?? "MeAjudaAi/1.0 (https://github.com/frigini/MeAjudaAi)";
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        });

        // Registrar serviços
        services.AddScoped<ICepLookupService, CepLookupService>();
        services.AddScoped<IGeocodingService, GeocodingService>();

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
