using MeAjudaAi.Web.Admin.Services;
using MeAjudaAi.Web.Admin.Services.Resilience.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Refit;

namespace MeAjudaAi.Web.Admin.Extensions;

/// <summary>
/// Métodos de extensão para IServiceCollection para simplificar o registro de clientes de API.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra um cliente de API Refit com configuração padrão (endereço base, handler de autenticação e políticas de resiliência).
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

        // Adiciona políticas de resiliência baseadas no tipo de operação
        if (useUploadPolicy)
        {
            // Política para uploads: sem retry, timeout estendido, circuit breaker
            httpClientBuilder.AddStandardResilienceHandler(options =>
            {
                // Desabilita retry para uploads (evita duplicação)
                options.Retry.MaxRetryAttempts = 0;
                
                // Configura circuit breaker e timeout
                // Nota: NullLogger usado aqui descarta eventos de política (OnOpened, OnClosed, etc.)
                // PollyLoggingHandler registra apenas requisições HTTP, não eventos internos da política
                ResiliencePolicies.ConfigureCircuitBreaker(options.CircuitBreaker, NullLogger.Instance);
                ResiliencePolicies.ConfigureUploadTimeout(options.TotalRequestTimeout);
            });
        }
        else
        {
            // Política padrão: retry + circuit breaker + timeout
            httpClientBuilder.AddStandardResilienceHandler(options =>
            {
                // Nota: NullLogger usado aqui descarta eventos de política (OnRetry, OnOpened, OnClosed, etc.)
                // PollyLoggingHandler registra apenas requisições HTTP, não eventos internos da política
                ResiliencePolicies.ConfigureRetry(options.Retry, NullLogger.Instance);
                ResiliencePolicies.ConfigureCircuitBreaker(options.CircuitBreaker, NullLogger.Instance);
                ResiliencePolicies.ConfigureTimeout(options.TotalRequestTimeout);
            });
        }
        
        return services;
    }
}
