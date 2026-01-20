using MeAjudaAi.Web.Admin.Services;
using MeAjudaAi.Web.Admin.Services.Resilience.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;
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
            // Política para uploads: retry mínimo (evita duplicação), timeout estendido, circuit breaker
            httpClientBuilder.AddStandardResilienceHandler(options =>
            {
                // Retry mínimo (validação requer >= 1) mas ShouldHandle evita tentativas em uploads
                options.Retry.MaxRetryAttempts = 1;
                options.Retry.ShouldHandle = _ => PredicateResult.False();
                
                // Configura circuit breaker e timeout
                // Logger injetado via ConfigurePrimaryHttpMessageHandler abaixo
                ResiliencePolicies.ConfigureCircuitBreaker(options.CircuitBreaker, NullLogger.Instance);
                ResiliencePolicies.ConfigureUploadTimeout(options.TotalRequestTimeout);
            });
        }
        else
        {
            // Política padrão: retry + circuit breaker + timeout
            httpClientBuilder.AddStandardResilienceHandler(options =>
            {
                // Logger injetado via ConfigurePrimaryHttpMessageHandler abaixo
                ResiliencePolicies.ConfigureRetry(options.Retry, NullLogger.Instance);
                ResiliencePolicies.ConfigureCircuitBreaker(options.CircuitBreaker, NullLogger.Instance);
                ResiliencePolicies.ConfigureTimeout(options.TotalRequestTimeout);
            });
        }

        // TODO: Migrar logging de eventos Polly (OnRetry, OnOpened, etc.) para usar ILogger do DI
        // LIMITATION: AddStandardResilienceHandler não suporta injeção de IServiceProvider
        // OPTIONS:
        //   1. Usar ConfigurePrimaryHttpMessageHandler com factory que recebe IServiceProvider
        //   2. Aguardar suporte em versão futura do Microsoft.Extensions.Http.Resilience
        //   3. Implementar custom DelegatingHandler que envolve policies manualmente
        // CURRENT: PollyLoggingHandler registra requisições HTTP (não eventos de política)
        
        return services;
    }
}
