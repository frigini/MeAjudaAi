using Aspire.Hosting.ApplicationModel;

namespace MeAjudaAi.AppHost.Resources;

/// <summary>
/// Recurso sentinela que representa a conclusão da configuração (bootstrap) do Keycloak.
/// Outros recursos podem dar um .WaitFor() neste recurso para garantir que os clientes OIDC existam.
/// </summary>
public class KeycloakBootstrapResource(string name) : Resource(name), IResource
{
}
