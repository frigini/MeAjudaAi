using MeAjudaAi.Gateway.Middlewares;
using MeAjudaAi.Gateway.Options;
using System.Net;

namespace MeAjudaAi.Gateway.Handlers;

/// <summary>
/// DelegatingHandler responsável por implementar a política de retry em requisições HTTP,
/// tratando falhas transitórias e timeouts.
/// </summary>
internal sealed class RetryDelegatingHandler(GatewayResilienceOptions options, ILogger? logger = null) : DelegatingHandler
{
    private static readonly IList<string> CachedDefaultRetryableMethods = new List<string> { "GET", "HEAD", "OPTIONS" }.AsReadOnly();
    
    private readonly GatewayResilienceOptions _options = options;
    private readonly IList<string> _retryableMethods = options.RetryableMethods?.Count > 0 ? options.RetryableMethods : CachedDefaultRetryableMethods;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Content != null)
        {
            await request.Content.LoadIntoBufferAsync(cancellationToken);
        }

        var attempt = 0;
        while (true)
        {
            HttpResponseMessage? response = null;
            try
            {
                response = await base.SendAsync(request, cancellationToken);
                if (attempt >= _options.RetryCount || !ShouldRetry(request, response, null))
                {
                    return response;
                }
            }
            catch (Exception ex) when (attempt < _options.RetryCount && (IsTransientException(ex) || ex is OperationCanceledException))
            {
                logger?.LogWarning(ex, "Transient exception on attempt {Attempt}/{MaxAttempts} for {Method} {Uri}",
                    attempt + 1, _options.RetryCount, request.Method, request.RequestUri);

                response?.Dispose();
                if (ex is OperationCanceledException && cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
            }
            catch (Exception)
            {
                response?.Dispose();
                throw;
            }

            attempt++;
            var delay = TimeSpan.FromMilliseconds(_options.RetryBaseDelayMs * Math.Pow(2, attempt));

            logger?.LogDebug("Retry attempt {Attempt}/{MaxAttempts} for {Method} {Uri} after {Delay}ms backoff",
                attempt, _options.RetryCount, request.Method, request.RequestUri, delay.TotalMilliseconds);

            response?.Dispose();
            await Task.Delay(delay, cancellationToken);
            }
            }
    private bool ShouldRetry(HttpRequestMessage request, HttpResponseMessage? response, Exception? exception)
    {
        if (!_retryableMethods.Contains(request.Method.Method, StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        if (response != null)
        {
            return IsTransient(response);
        }

    return exception != null && IsTransientException(exception);
    }

    private static bool IsTransient(HttpResponseMessage response) =>
        (int)response.StatusCode >= 500 ||
        response.StatusCode is HttpStatusCode.TooManyRequests or HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout;

    private static bool IsTransientException(Exception ex) =>
        ex is HttpRequestException or TaskCanceledException or IOException;
}
