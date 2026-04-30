using System.Net;
using MeAjudaAi.Gateway.Options;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Forwarder;

namespace MeAjudaAi.Gateway.Middlewares;

public class ResilientForwarderHttpClientFactory : IForwarderHttpClientFactory
{
    private readonly GatewayResilienceOptions _options;
    private readonly ILogger<ResilientForwarderHttpClientFactory> _logger;
    private static readonly HashSet<string> DefaultRetryableMethods =
        new(["GET", "HEAD", "OPTIONS"], StringComparer.OrdinalIgnoreCase);

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
            return new RetryDelegatingHandler(_options, _logger) { InnerHandler = socketsHandler };
        }

        return socketsHandler;
    }
}

internal sealed class RetryDelegatingHandler : DelegatingHandler
{
    private readonly GatewayResilienceOptions _options;
    private readonly ILogger<ResilientForwarderHttpClientFactory> _logger;
    private static readonly HashSet<string> DefaultRetryableMethods =
        new(["GET", "HEAD", "OPTIONS"], StringComparer.OrdinalIgnoreCase);

    public RetryDelegatingHandler(GatewayResilienceOptions options, ILogger<ResilientForwarderHttpClientFactory> logger)
    {
        _options = options;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var retryableMethods = _options.RetryableMethods?.Count > 0 
            ? _options.RetryableMethods 
            : DefaultRetryableMethods.ToList();
        
        var allowRetry = retryableMethods.Contains(request.Method.Method, StringComparer.OrdinalIgnoreCase);
        
        if (!allowRetry || _options.RetryCount <= 0)
        {
            return await base.SendAsync(request, ct);
        }

        HttpResponseMessage? last = null;
        for (int attempt = 0; attempt <= _options.RetryCount; attempt++)
        {
            last = await base.SendAsync(request, ct);
            
            if (!IsTransient(last))
            {
                return last;
            }

            _logger.LogWarning(
                "Retry attempt {AttemptNumber}/{MaxAttempts} for {Method} {Url} - Status: {StatusCode}",
                attempt + 1,
                _options.RetryCount,
                request.Method.Method,
                request.RequestUri,
                last.StatusCode);

            if (attempt < _options.RetryCount)
            {
                var delay = TimeSpan.FromMilliseconds(_options.RetryBaseDelayMs * Math.Pow(2, attempt));
                await Task.Delay(delay, ct);
            }
        }

        return last!;
    }

    private static bool IsTransient(HttpResponseMessage response) =>
        (int)response.StatusCode >= 500 ||
        response.StatusCode is HttpStatusCode.TooManyRequests or HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout;
}