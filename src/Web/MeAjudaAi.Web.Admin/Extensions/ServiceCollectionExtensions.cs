using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Refit;

namespace MeAjudaAi.Web.Admin.Extensions;

/// <summary>
/// Métodos de extensão para IServiceCollection para simplificar o registro de clientes de API.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra um cliente de API Refit com configuração padrão (endereço base e handler de autenticação).
    /// </summary>
    /// <typeparam name="TClient">O tipo da interface Refit a ser registrada.</typeparam>
    /// <param name="services">A coleção de serviços.</param>
    /// <param name="baseUrl">A URL base da API.</param>
    /// <returns>A coleção de serviços para encadeamento.</returns>
    public static IServiceCollection AddApiClient<TClient>(
        this IServiceCollection services, 
        string baseUrl) where TClient : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseUrl, nameof(baseUrl));

        services.AddRefitClient<TClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl))
            .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();
        
        return services;
    }
}
