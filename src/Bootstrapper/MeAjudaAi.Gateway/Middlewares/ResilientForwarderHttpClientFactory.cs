using System.Net;
using MeAjudaAi.Gateway.Options;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Forwarder;

namespace MeAjudaAi.Gateway.Middlewares;

public class ResilientForwarderHttpClientFactory : IForwarderHttpClientFactory
{
    private readonly GatewayResilienceOptions _options;
    private readonly ILogger<ResilientForwarderHttpClientFactory> _logger;

    public ResilientForwarderHttpClientFactory(
        IOptions<GatewayResilienceOptions> options,
        ILogger<ResilientForwarderHttpClientFactory> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public HttpMessageInvoker CreateClient(ForwarderHttpClientContext context)
    {
        var handler = CreateHandler(context);
        return new HttpMessageInvoker(handler);
    }

    public HttpMessageHandler CreateHandler(ForwarderHttpClientContext context)
    {
        _logger.LogDebug(
            "Creating resilient HttpHandler with timeout {Timeout}s and retry count {RetryCount}",
            _options.TimeoutSeconds,
            _options.RetryCount);

        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
            MaxConnectionsPerServer = 100,
            EnableMultipleHttp2Connections = true,
            ConnectTimeout = TimeSpan.FromSeconds(_options.TimeoutSeconds),
            ResponseDrainTimeout = TimeSpan.FromSeconds(_options.TimeoutSeconds)
        };

        return handler;
    }
}