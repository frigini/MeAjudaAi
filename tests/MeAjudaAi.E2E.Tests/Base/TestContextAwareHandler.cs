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
        var logEntry = $"[{DateTime.UtcNow:O}] {request.Method} {request.RequestUri}\n";
        
        // Preflight requests (OPTIONS) não requerem autenticação - são parte do protocolo CORS
        if (request.Method == HttpMethod.Options)
        {
            await File.AppendAllTextAsync(diagPath, logEntry + "  -> OPTIONS (skipping auth)\n");
            return await base.SendAsync(request, cancellationToken);
        }
        
        // Verificar se é um endpoint público que não requer autenticação
        var requestPath = request.RequestUri?.AbsolutePath ?? string.Empty;
        var isPublicEndpoint = PublicEndpoints.Any(endpoint => 
            requestPath.Equals(endpoint, StringComparison.OrdinalIgnoreCase) ||
            requestPath.StartsWith(endpoint + "/", StringComparison.OrdinalIgnoreCase));

        // Injetar localização para testes (Muriaé/MG é uma cidade permitida na configuração)
        request.Headers.Remove("X-User-City");
        request.Headers.Add("X-User-City", "Muriaé");
        request.Headers.Remove("X-User-State");
        request.Headers.Add("X-User-State", "MG");

        // Obter contexto AsyncLocal IMEDIATAMENTE antes do envio (ainda no contexto do teste)
        var contextId = ConfigurableTestAuthenticationHandler.GetCurrentTestContextId();
        
        await File.AppendAllTextAsync(diagPath, logEntry + $"  -> Context: {contextId ?? "NULL"} (IsPublic: {isPublicEndpoint})\n");

        // Se for endpoint público E não tiver contexto, permitir (request anônimo)
        if (isPublicEndpoint && string.IsNullOrEmpty(contextId))
        {
            var publicResponse = await base.SendAsync(request, cancellationToken);
            await File.AppendAllTextAsync(diagPath, $"[{DateTime.UtcNow:O}] {request.RequestUri} -> {publicResponse.StatusCode}\n");
            return publicResponse;
        }
        
        // Se NÃO for endpoint público e não tiver contexto, ERRO
        if (string.IsNullOrEmpty(contextId))
        {
            var errorMsg = $"❌ AUTHENTICATION CONTEXT NOT FOUND!\n" +
                $"The TestContextAwareHandler was called but GetCurrentTestContextId() returned null/empty.\n" +
                $"This means AuthenticateAsAdmin() was NOT called before making the HTTP request, OR AsyncLocal context was lost.\n" +
                $"URL: {request.RequestUri}\n\n" +
                $"FIX: Ensure you call TestContainerFixture.AuthenticateAsAdmin() BEFORE making HTTP requests in your test.";
            
            await File.AppendAllTextAsync(diagPath, logEntry + "  -> ERROR: Auth context missing\n");
            throw new InvalidOperationException(errorMsg);
        }
        
        // Add header if not already present
        if (!request.Headers.Contains(ConfigurableTestAuthenticationHandler.TestContextHeader))
        {
            request.Headers.Add(ConfigurableTestAuthenticationHandler.TestContextHeader, contextId);
        }

        var response = await base.SendAsync(request, cancellationToken);
        await File.AppendAllTextAsync(diagPath, $"[{DateTime.UtcNow:O}] {request.RequestUri} -> {response.StatusCode}\n");
        return response;
    }
}
