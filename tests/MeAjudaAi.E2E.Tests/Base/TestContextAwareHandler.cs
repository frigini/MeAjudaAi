using MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers;

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
        "/api/users/register",
        "/api/users/login",
        "/api/v1/users/register",
        "/api/v1/users/login",
        "/api/v1/service-catalogs/services",
        "/api/v1/service-catalogs/categories",
        "/_framework",
        "/_vs"
    ];

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Preflight requests (OPTIONS) não requerem autenticação - são parte do protocolo CORS
        if (request.Method == HttpMethod.Options)
        {
            return await base.SendAsync(request, cancellationToken);
        }
        
        // Verificar se é um endpoint público que não requer autenticação
        var requestPath = request.RequestUri?.AbsolutePath ?? string.Empty;
        var isPublicEndpoint = PublicEndpoints.Any(endpoint => 
            requestPath.Equals(endpoint, StringComparison.OrdinalIgnoreCase) ||
            requestPath.StartsWith(endpoint + "/", StringComparison.OrdinalIgnoreCase));

        // Obter contexto AsyncLocal IMEDIATAMENTE antes do envio (ainda no contexto do teste)
        var contextId = ConfigurableTestAuthenticationHandler.GetCurrentTestContextId();
        
        // Se for endpoint público E não tiver contexto, permitir (request anônimo)
        if (isPublicEndpoint && string.IsNullOrEmpty(contextId))
        {
            return await base.SendAsync(request, cancellationToken);
        }
        
        // Se NÃO for endpoint público e não tiver contexto, ERRO
        if (string.IsNullOrEmpty(contextId))
        {
            throw new InvalidOperationException(
                $"❌ AUTHENTICATION CONTEXT NOT FOUND!\n" +
                $"The TestContextAwareHandler was called but GetCurrentTestContextId() returned null/empty.\n" +
                $"This means AuthenticateAsAdmin() was NOT called before making the HTTP request, OR AsyncLocal context was lost.\n" +
                $"URL: {request.RequestUri}\n\n" +
                $"FIX: Ensure you call TestContainerFixture.AuthenticateAsAdmin() BEFORE making HTTP requests in your test.");
        }
        
        // Add header if not already present
        if (!request.Headers.Contains(ConfigurableTestAuthenticationHandler.TestContextHeader))
        {
            request.Headers.Add(ConfigurableTestAuthenticationHandler.TestContextHeader, contextId);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
