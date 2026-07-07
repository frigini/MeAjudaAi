using MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers;

/// <summary>
/// HTTP message handler that injects test context ID header into all requests.
/// Checks for public endpoints that don't require authentication context.
/// </summary>
public class TestContextAwareHandler : DelegatingHandler
{
    private static readonly string[] PublicEndpoints = ["/health", "/alive", "/api/v1/service-catalogs/services", "/api/v1/service-catalogs/categories", "/_framework", "/_vs"];

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Method == HttpMethod.Options)
            return await base.SendAsync(request, cancellationToken);

        var requestPath = request.RequestUri?.AbsolutePath ?? string.Empty;
        var isPublicEndpoint = PublicEndpoints.Any(endpoint =>
            requestPath.Equals(endpoint, StringComparison.OrdinalIgnoreCase) ||
            requestPath.StartsWith(endpoint + "/", StringComparison.OrdinalIgnoreCase));

        var contextId = ConfigurableTestAuthenticationHandler.GetCurrentTestContextId();

        if (isPublicEndpoint && string.IsNullOrEmpty(contextId))
            return await base.SendAsync(request, cancellationToken);

        if (string.IsNullOrEmpty(contextId))
            throw new InvalidOperationException(
                $"AUTHENTICATION CONTEXT NOT FOUND! URL: {request.RequestUri}");

        if (!request.Headers.Contains(ConfigurableTestAuthenticationHandler.TestContextHeader))
            request.Headers.Add(ConfigurableTestAuthenticationHandler.TestContextHeader, contextId);

        return await base.SendAsync(request, cancellationToken);
    }
}
