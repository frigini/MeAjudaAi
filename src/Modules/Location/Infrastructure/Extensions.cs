using MeAjudaAi.Modules.Location.Application.ModuleApi;
using MeAjudaAi.Modules.Location.Application.Services;
using MeAjudaAi.Modules.Location.Infrastructure.ExternalApis.Clients;
using MeAjudaAi.Modules.Location.Infrastructure.Services;
using MeAjudaAi.Shared.Contracts.Modules.Location;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Location.Infrastructure;

/// <summary>
/// Métodos de extensão para registrar serviços do módulo Location.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Registra todos os serviços do módulo Location.
    /// </summary>
    public static IServiceCollection AddLocationModule(this IServiceCollection services)
    {
        // Registrar HTTP clients para APIs de CEP
        // ServiceDefaults já configura resiliência (retry, circuit breaker, timeout)
        services.AddHttpClient<ViaCepClient>();
        services.AddHttpClient<BrasilApiCepClient>();
        services.AddHttpClient<OpenCepClient>();
        
        // Registrar HTTP client para Nominatim (geocoding)
        services.AddHttpClient<NominatimClient>();

        // Registrar serviços
        services.AddScoped<ICepLookupService, CepLookupService>();
        services.AddScoped<IGeocodingService, GeocodingService>();

        // Registrar Module API
        services.AddScoped<ILocationModuleApi, LocationsModuleApi>();

        return services;
    }
}
