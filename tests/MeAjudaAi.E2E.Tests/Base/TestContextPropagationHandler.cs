using MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers;

namespace MeAjudaAi.E2E.Tests.Base;

/// <summary>
/// HttpMessageHandler que propaga o contexto de teste via header X-Test-Context-Id.
/// Isso permite que a autenticação configurada via AsyncLocal seja propagada para requisições HTTP.
/// </summary>
public class TestContextPropagationHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Obter o contexto de teste atual do AsyncLocal via ConfigurableTestAuthenticationHandler
        var contextId = ConfigurableTestAuthenticationHandler.GetCurrentTestContextId();
        
        if (!string.IsNullOrEmpty(contextId))
        {
            // Adicionar header para propagar contexto para o servidor
            request.Headers.Add(ConfigurableTestAuthenticationHandler.TestContextHeader, contextId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
