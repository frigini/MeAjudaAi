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
        var invoker = new HttpMessageInvoker(handler);
        return invoker;
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
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

        var retryableMethods = _options.RetryableMethods?.Count > 0 
            ? _options.RetryableMethods 
            : DefaultRetryableMethods.ToList();
        
        var allowRetry = retryableMethods.Contains(request.Method.Method, StringComparer.OrdinalIgnoreCase);
        
        if (!allowRetry || _options.RetryCount <= 0)
        {
            return await base.SendAsync(request, timeoutCts.Token);
        }

        HttpResponseMessage? last = null;
        Exception? lastException = null;
        
        for (int attempt = 0; attempt <= _options.RetryCount; attempt++)
        {
            try
            {
                last = await base.SendAsync(request, timeoutCts.Token);
                
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
                    last.Dispose();
                    last = null;
                }
            }
            catch (Exception ex) when (IsTransientException(ex))
            {
                lastException = ex;
                _logger.LogWarning(
                    "Retry attempt {AttemptNumber}/{MaxAttempts} for {Method} {Url} - Exception: {Message}",
                    attempt + 1,
                    _options.RetryCount,
                    request.Method.Method,
                    request.RequestUri,
                    ex.Message);
            }

            if (attempt < _options.RetryCount)
            {
                var delay = TimeSpan.FromMilliseconds(_options.RetryBaseDelayMs * Math.Pow(2, attempt));
                await Task.Delay(delay, timeoutCts.Token);
            }
        }

        if (last != null)
        {
            return last;
        }

        if (lastException != null)
        {
            throw lastException;
        }

        throw new HttpRequestException("All retry attempts failed");
    }

    private static bool IsTransient(HttpResponseMessage response) =>
        (int)response.StatusCode >= 500 ||
        response.StatusCode is HttpStatusCode.TooManyRequests or HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout;

    private static bool IsTransientException(Exception ex) =>
        ex is HttpRequestException or TaskCanceledException or IOException;
}