using MeAjudaAi.Gateway.Middlewares;
using MeAjudaAi.Gateway.Options;
using System.Net;

namespace MeAjudaAi.Gateway.Handlers;

/// <summary>
/// DelegatingHandler responsável por implementar a política de retry em requisições HTTP,
/// tratando falhas transitórias e timeouts.
/// </summary>
internal sealed class RetryDelegatingHandler(GatewayResilienceOptions options, ILogger<ResilientForwarderHttpClientFactory> logger) : DelegatingHandler
{
    private static readonly HashSet<string> DefaultRetryableMethods =
        new(["GET", "HEAD", "OPTIONS"], StringComparer.OrdinalIgnoreCase);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(options.TimeoutSeconds));

        var retryableMethods = options.RetryableMethods?.Count > 0 
            ? options.RetryableMethods 
            : DefaultRetryableMethods.ToList();
        
        var allowRetry = retryableMethods.Contains(request.Method.Method, StringComparer.OrdinalIgnoreCase);
        
        if (!allowRetry || options.RetryCount <= 0)
        {
            return await base.SendAsync(request, timeoutCts.Token);
        }

        HttpResponseMessage? last = null;
        Exception? lastException = null;
        
        for (int attempt = 0; attempt <= options.RetryCount; attempt++)
        {
            try
            {
                last = await base.SendAsync(request, timeoutCts.Token);
                
                if (!IsTransient(last))
                {
                    return last;
                }

                logger.LogWarning(
                    "Retry attempt {AttemptNumber}/{MaxAttempts} for {Method} {Url} - Status: {StatusCode}",
                    attempt + 1,
                    options.RetryCount,
                    request.Method.Method,
                    request.RequestUri,
                    last.StatusCode);

                if (attempt < options.RetryCount)
                {
                    last.Dispose();
                    last = null;
                }
            }
            catch (Exception ex) when (IsTransientException(ex))
            {
                lastException = ex;
                logger.LogWarning(
                    "Retry attempt {AttemptNumber}/{MaxAttempts} for {Method} {Url} - Exception: {Message}",
                    attempt + 1,
                    options.RetryCount,
                    request.Method.Method,
                    request.RequestUri,
                    ex.Message);
            }

            if (attempt < options.RetryCount)
            {
                var delay = TimeSpan.FromMilliseconds(options.RetryBaseDelayMs * Math.Pow(2, attempt));
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