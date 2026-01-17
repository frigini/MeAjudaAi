using MeAjudaAi.Web.Admin.Services;
using MeAjudaAi.Web.Admin.Services.Resilience;
using Refit;

namespace MeAjudaAi.Web.Admin.Extensions;

/// <summary>
/// Métodos de extensão para IServiceCollection para simplificar o registro de clientes de API.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra um cliente de API Refit com configuração padrão (endereço base, handler de autenticação e políticas Polly).
    /// </summary>
    /// <typeparam name="TClient">O tipo da interface Refit a ser registrada.</typeparam>
    /// <param name="services">A coleção de serviços.</param>
    /// <param name="baseUrl">A URL base da API.</param>
    /// <param name="useUploadPolicy">Se verdadeiro, usa política otimizada para uploads (sem retry). Padrão: false.</param>
    /// <returns>A coleção de serviços para encadeamento.</returns>
    public static IServiceCollection AddApiClient<TClient>(
        this IServiceCollection services, 
        string baseUrl,
        bool useUploadPolicy = false) where TClient : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseUrl, nameof(baseUrl));
        
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException($"The value '{baseUrl}' is not a valid absolute URI.", nameof(baseUrl));
        }

        var httpClientBuilder = services.AddRefitClient<TClient>()
            .ConfigureHttpClient(c => c.BaseAddress = uri)
            .AddHttpMessageHandler<ApiAuthorizationMessageHandler>()
            .AddHttpMessageHandler<PollyLoggingHandler>();

        // Adiciona políticas Polly baseadas no tipo de operação
        if (useUploadPolicy)
        {
            // Política para uploads: sem retry, timeout estendido
            httpClientBuilder.AddPolicyHandler((serviceProvider, request) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<TClient>>();
                return PollyPolicies.GetUploadPolicy(logger);
            });
        }
        else
        {
            // Política padrão: retry + circuit breaker + timeout
            httpClientBuilder.AddPolicyHandler((serviceProvider, request) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<TClient>>();
                return PollyPolicies.GetCombinedPolicy(logger);
            });
        }
        
        return services;
    }
}
