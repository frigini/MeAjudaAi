using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace MeAjudaAi.Web.Admin.Services.Resilience;

/// <summary>
/// Políticas de resiliência usando Microsoft.Extensions.Http.Resilience
/// </summary>
public static class ResiliencePolicies
{
    /// <summary>
    /// Configura retry com backoff exponencial
    /// 3 tentativas: aguarda 2s, 4s, 8s entre tentativas
    /// </summary>
    public static void ConfigureRetry(HttpRetryStrategyOptions options, ILogger logger)
    {
        options.MaxRetryAttempts = 3;
        options.BackoffType = DelayBackoffType.Exponential;
        options.Delay = TimeSpan.FromSeconds(2);
        options.ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .Handle<HttpRequestException>()
            .Handle<TimeoutRejectedException>()
            .HandleResult(response => 
                (int)response.StatusCode >= 500 || 
                response.StatusCode == System.Net.HttpStatusCode.RequestTimeout);
        
        options.OnRetry = args =>
        {
            logger.LogWarning(
                "⚠️ Retry {RetryCount}/{MaxRetryAttempts} after {DelaySeconds}s delay. Reason: {Reason}",
                args.AttemptNumber,
                options.MaxRetryAttempts,
                args.RetryDelay.TotalSeconds,
                args.Outcome.Exception?.Message ?? args.Outcome.Result?.StatusCode.ToString() ?? "Unknown");
            
            return default;
        };
    }

    /// <summary>
    /// Configura circuit breaker: abre após 5 falhas consecutivas, aguarda 30s antes de tentar novamente
    /// </summary>
    public static void ConfigureCircuitBreaker(HttpCircuitBreakerStrategyOptions options, ILogger logger)
    {
        options.FailureRatio = 0.5;
        options.MinimumThroughput = 5;
        options.SamplingDuration = TimeSpan.FromSeconds(30);
        options.BreakDuration = TimeSpan.FromSeconds(30);
        options.ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .Handle<HttpRequestException>()
            .Handle<TimeoutRejectedException>()
            .HandleResult(response => (int)response.StatusCode >= 500);
        
        options.OnOpened = args =>
        {
            logger.LogError(
                "🔴 Circuit breaker opened! Will retry after {BreakDuration}s. Reason: {Reason}",
                options.BreakDuration.TotalSeconds,
                args.Outcome.Exception?.Message ?? args.Outcome.Result?.StatusCode.ToString() ?? "Unknown");
            
            return default;
        };
        
        options.OnClosed = args =>
        {
            logger.LogInformation("✅ Circuit breaker reset - connection restored");
            return default;
        };
        
        options.OnHalfOpened = args =>
        {
            logger.LogInformation("🟡 Circuit breaker half-open - testing connection");
            return default;
        };
    }

    /// <summary>
    /// Configura timeout de 30 segundos para operações normais
    /// </summary>
    public static void ConfigureTimeout(HttpTimeoutStrategyOptions options)
    {
        options.Timeout = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Configura timeout estendido de 2 minutos para uploads de arquivos
    /// </summary>
    public static void ConfigureUploadTimeout(HttpTimeoutStrategyOptions options)
    {
        options.Timeout = TimeSpan.FromMinutes(2);
    }
}
