using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Refit;

namespace MeAjudaAi.Web.Admin.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to simplify API client registration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a Refit API client with standard configuration (base address and authentication handler).
    /// </summary>
    /// <typeparam name="TClient">The Refit interface type to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="baseUrl">The base URL for the API.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApiClient<TClient>(
        this IServiceCollection services, 
        string baseUrl) where TClient : class
    {
        services.AddRefitClient<TClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl))
            .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();
        
        return services;
    }
}
