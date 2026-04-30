using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Yarp.ReverseProxy.Forwarder;

namespace MeAjudaAi.Gateway.Resilience;

/// <summary>
/// Fábrica de clientes HTTP para o YARP com políticas de resiliência Polly (timeout + retry).
/// Substitui o <see cref="ForwarderHttpClientFactory"/> padrão do YARP, adicionando:
/// <list type="bullet">
///   <item>Timeout global por requisição (configurável via GatewayResilience:TimeoutSeconds)</item>
///   <item>Retry com backoff exponencial em falhas transientes (configurável via GatewayResilience:RetryCount)</item>
/// </list>
/// </summary>
public class ResilientForwarderHttpClientFactory : ForwarderHttpClientFactory
{
    private readonly IOptionsMonitor<GatewayResilienceOptions> _options;
    private readonly ILogger<ResilientForwarderHttpClientFactory> _logger;

    public ResilientForwarderHttpClientFactory(
        IOptionsMonitor<GatewayResilienceOptions> options,
        ILogger<ResilientForwarderHttpClientFactory> logger)
    {
        _options = options;
        _logger = logger;
    }

    protected override HttpMessageHandler CreateHandler(ForwarderHttpClientContext context)
    {
        var opts = _options.CurrentValue;

        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: opts.RetryCount,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timeSpan, retryAttempt, _) =>
                {
                    var reason = outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString();
                    _logger.LogWarning(
                        "Retry {RetryAttempt}/{MaxRetries} após {DelaySeconds:F1}s. Motivo: {Reason}",
                        retryAttempt, opts.RetryCount, timeSpan.TotalSeconds, reason);
                });

        var timeoutPolicy = Policy
            .TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(opts.TimeoutSeconds));

        // Timeout é a política externa (wraps retry): garante timeout total mesmo com retentativas
        var resilientPolicy = Policy.WrapAsync(timeoutPolicy, retryPolicy);

        var baseHandler = base.CreateHandler(context);

        return new PolicyHttpMessageHandler(resilientPolicy)
        {
            InnerHandler = baseHandler
        };
    }
}