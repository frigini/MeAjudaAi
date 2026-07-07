using MeAjudaAi.Gateway.Handlers;
using MeAjudaAi.Gateway.Options;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Forwarder;

namespace MeAjudaAi.Gateway.Middlewares;

/// <summary>
/// Fábrica customizada para criar HttpMessageInvoker/HttpMessageHandler com políticas de
/// resiliência (retry, timeout) aplicadas ao encaminhamento de requisições pelo YARP.
/// </summary>
public class ResilientForwarderHttpClientFactory(
    IOptions<GatewayResilienceOptions> options,
    ILogger<ResilientForwarderHttpClientFactory> logger) : IForwarderHttpClientFactory
{
    private readonly GatewayResilienceOptions _options = options.Value;

    public HttpMessageInvoker CreateClient(ForwarderHttpClientContext context)
    {
        var handler = CreateHandler(context);
        var invoker = new HttpMessageInvoker(handler);
        return invoker;
    }

    public HttpMessageHandler CreateHandler(ForwarderHttpClientContext context)
    {
        logger.LogDebug(
            "Creating resilient HttpHandler with timeout {Timeout}s and retry count {RetryCount}",
            _options.TimeoutSeconds,
            _options.RetryCount);

        var socketsHandler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
            MaxConnectionsPerServer = 100,
            EnableMultipleHttp2Connections = true,
            ConnectTimeout = TimeSpan.FromSeconds(_options.TimeoutSeconds),
            ResponseDrainTimeout = TimeSpan.FromSeconds(_options.TimeoutSeconds)
        };

        if (_options.RetryCount > 0)
        {
            return new RetryDelegatingHandler(_options, logger) { InnerHandler = socketsHandler };
        }

        return socketsHandler;
    }
}
