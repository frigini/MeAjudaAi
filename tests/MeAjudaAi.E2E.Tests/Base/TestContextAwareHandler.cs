using MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers;
using System.IO;

namespace MeAjudaAi.E2E.Tests.Base;

/// <summary>
/// DelegatingHandler que automaticamente adiciona o header X-Test-Context-Id usando o contexto AsyncLocal atual.
/// Necessário porque AsyncLocal não atravessa boundary do HttpClient/TestServer automaticamente.
/// Este handler intercepta TODAS as requests e injeta o header de contexto.
/// </summary>
public class TestContextAwareHandler : DelegatingHandler
{
    // Endpoints que NÃO requerem autenticação (são públicos)
    private static readonly string[] PublicEndpoints =
    [
        "/health",
        "/alive",
        "/api/v1/service-catalogs/services",
        "/api/v1/service-catalogs/categories",
        "/_framework",
        "/_vs"
    ];

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var diagPath = Path.Combine(AppContext.BaseDirectory, "http_diag.log");
        var startTime = DateTime.UtcNow;
        var logEntry = $"[{startTime:O}] {request.Method} {request.RequestUri}";

        try
        {
            if (request.Method == HttpMethod.Options)
            {
                await File.AppendAllTextAsync(diagPath, logEntry + " -> OPTIONS (skipping auth)\n").ConfigureAwait(false);
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }

            var requestPath = request.RequestUri?.AbsolutePath ?? string.Empty;
            var isPublicEndpoint = PublicEndpoints.Any(endpoint =>
                requestPath.Equals(endpoint, StringComparison.OrdinalIgnoreCase) ||
                requestPath.StartsWith(endpoint + "/", StringComparison.OrdinalIgnoreCase));

            request.Headers.Remove("X-User-City");
            request.Headers.Add("X-User-City", "Muriaé");
            request.Headers.Remove("X-User-State");
            request.Headers.Add("X-User-State", "MG");

            var contextId = ConfigurableTestAuthenticationHandler.GetCurrentTestContextId();

            await File.AppendAllTextAsync(diagPath, $"{logEntry} START Context: {contextId ?? "NULL"} (IsPublic: {isPublicEndpoint})\n").ConfigureAwait(false);

            if (isPublicEndpoint && string.IsNullOrEmpty(contextId))
            {
                var publicResponse = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                await File.AppendAllTextAsync(diagPath, $"[{DateTime.UtcNow:O}] {request.RequestUri} -> {publicResponse.StatusCode} (elapsed: {(DateTime.UtcNow - startTime).TotalSeconds:F1}s)\n").ConfigureAwait(false);
                return publicResponse;
            }

            if (string.IsNullOrEmpty(contextId))
            {
                var errorMsg = $"AUTHENTICATION CONTEXT NOT FOUND! URL: {request.RequestUri}";
                await File.AppendAllTextAsync(diagPath, $"{logEntry} ERROR: Auth context missing\n").ConfigureAwait(false);
                throw new InvalidOperationException(errorMsg);
            }

            if (!request.Headers.Contains(ConfigurableTestAuthenticationHandler.TestContextHeader))
            {
                request.Headers.Add(ConfigurableTestAuthenticationHandler.TestContextHeader, contextId);
            }

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(120));

            HttpResponseMessage? response = null;
            Exception? caughtEx = null;
            try
            {
                response = await base.SendAsync(request, cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token)
            {
                var elapsed = (DateTime.UtcNow - startTime).TotalSeconds;
                await File.AppendAllTextAsync(diagPath, $"[{DateTime.UtcNow:O}] {request.Method} {request.RequestUri} TIMEOUT after {elapsed:F1}s (ct: {cancellationToken.IsCancellationRequested})\n").ConfigureAwait(false);
                throw new TimeoutException($"HTTP {request.Method} {request.RequestUri} timed out after {elapsed:F1}s", ex);
            }
            catch (Exception ex)
            {
                caughtEx = ex;
                throw;
            }
            finally
            {
                var elapsed = (DateTime.UtcNow - startTime).TotalSeconds;
                var statusInfo = response != null ? $"{response.StatusCode}" : (caughtEx != null ? $"ERROR: {caughtEx.Message}" : "UNKNOWN");
                await File.AppendAllTextAsync(diagPath, $"[{DateTime.UtcNow:O}] {request.Method} {request.RequestUri} -> {statusInfo} (elapsed: {elapsed:F1}s)\n").ConfigureAwait(false);
            }

            await File.AppendAllTextAsync(diagPath, $"[{DateTime.UtcNow:O}] {request.RequestUri} -> {response.StatusCode} (elapsed: {(DateTime.UtcNow - startTime).TotalSeconds:F1}s)\n").ConfigureAwait(false);
            return response;
        }
        catch (Exception ex) when (ex is not InvalidOperationException && ex is not TimeoutException)
        {
            await File.AppendAllTextAsync(diagPath, $"[{DateTime.UtcNow:O}] ERROR: {ex.Message}\n").ConfigureAwait(false);
            throw;
        }
    }
}
