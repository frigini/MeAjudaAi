using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace MeAjudaAi.Web.Admin.Services.Resilience;

/// <summary>
/// Políticas de resiliência Polly para chamadas HTTP
/// </summary>
public static class PollyPolicies
{
    /// <summary>
    /// Política de retry com backoff exponencial
    /// 3 tentativas: aguarda 2s, 4s, 8s entre tentativas
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ILogger logger)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError() // 5xx e 408
            .Or<TimeoutRejectedException>() // Timeout do Polly
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    var requestUri = context.TryGetValue("requestUri", out var uri) 
                        ? uri?.ToString() 
                        : "Unknown";
                    
                    logger.LogWarning(
                        "⚠️ Retry {RetryCount}/3 after {DelaySeconds}s delay. " +
                        "Request: {RequestUri}. Reason: {Reason}",
                        retryCount,
                        timespan.TotalSeconds,
                        requestUri,
                        outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString() ?? "Unknown");
                });
    }

    /// <summary>
    /// Circuit breaker: abre após 5 falhas consecutivas, aguarda 30s antes de tentar novamente
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(ILogger logger)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, duration) =>
                {
                    logger.LogError(
                        "🔴 Circuit breaker opened! Will retry after {BreakDuration}s. " +
                        "Reason: {Reason}",
                        duration.TotalSeconds,
                        outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString() ?? "Unknown");
                },
                onReset: () =>
                {
                    logger.LogInformation("✅ Circuit breaker reset - connection restored");
                },
                onHalfOpen: () =>
                {
                    logger.LogInformation("🟡 Circuit breaker half-open - testing connection");
                });
    }

    /// <summary>
    /// Timeout de 30 segundos para operações normais
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(
            timeout: TimeSpan.FromSeconds(30),
            timeoutStrategy: TimeoutStrategy.Optimistic);
    }

    /// <summary>
    /// Timeout estendido de 2 minutos para uploads de arquivos
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetUploadTimeoutPolicy()
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(
            timeout: TimeSpan.FromMinutes(2),
            timeoutStrategy: TimeoutStrategy.Optimistic);
    }

    /// <summary>
    /// Política combinada: Timeout -> Retry -> Circuit Breaker
    /// A ordem é importante: timeout externo, retry no meio, circuit breaker interno
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy(ILogger logger)
    {
        return Policy.WrapAsync(
            GetRetryPolicy(logger),
            GetCircuitBreakerPolicy(logger),
            GetTimeoutPolicy());
    }

    /// <summary>
    /// Política para uploads: sem retry (para evitar uploads duplicados), mas com circuit breaker e timeout estendido
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetUploadPolicy(ILogger logger)
    {
        return Policy.WrapAsync(
            GetCircuitBreakerPolicy(logger),
            GetUploadTimeoutPolicy());
    }
}
