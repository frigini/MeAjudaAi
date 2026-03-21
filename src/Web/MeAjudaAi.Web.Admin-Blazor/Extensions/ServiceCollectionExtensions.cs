using MeAjudaAi.Web.Admin.Services;
using MeAjudaAi.Web.Admin.Services.Resilience.Http;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
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
        ArgumentException.ThrowIfNullOrWhiteSpace(baseUrl);
        
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException($"The value '{baseUrl}' is not a valid absolute URI.", nameof(baseUrl));
        }

        var httpClientBuilder = services.AddRefitClient<TClient>()
            .ConfigureHttpClient(c => c.BaseAddress = uri)
            .AddHttpMessageHandler<ApiAuthorizationMessageHandler>()
            .AddHttpMessageHandler<PollyLoggingHandler>();

        // Adiciona políticas de resiliência baseadas no tipo de operação
        var resilienceBuilder = httpClientBuilder.AddStandardResilienceHandler();
        
        services.AddOptions<HttpStandardResilienceOptions>(resilienceBuilder.PipelineName)
            .Configure<IServiceProvider>((options, sp) =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger(typeof(ResiliencePolicies));

                if (useUploadPolicy)
                {
                    // Política para uploads: retry mínimo (evita duplicação), timeout estendido, circuit breaker
                    options.Retry.MaxRetryAttempts = 1;
                    options.Retry.ShouldHandle = _ => PredicateResult.False();
                    
                    ResiliencePolicies.ConfigureCircuitBreaker(options.CircuitBreaker, logger);
                    ResiliencePolicies.ConfigureUploadTimeout(options.TotalRequestTimeout);
                }
                else
                {
                    // Política padrão: retry + circuit breaker + timeout
                    ResiliencePolicies.ConfigureRetry(options.Retry, logger);
                    ResiliencePolicies.ConfigureCircuitBreaker(options.CircuitBreaker, logger);
                    ResiliencePolicies.ConfigureTimeout(options.TotalRequestTimeout);
                }
            });
        
        return services;
    }
}
