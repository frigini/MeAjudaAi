using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MeAjudaAi.Contracts.Configuration;

namespace MeAjudaAi.Web.Admin.Services;

/// <summary>
/// Handler customizado para anexar token de autenticação nas requisições à API.
/// Configurado para aceitar qualquer porta localhost para suportar Aspire com portas dinâmicas.
/// </summary>
public class ApiAuthorizationMessageHandler : AuthorizationMessageHandler
{
    public ApiAuthorizationMessageHandler(IAccessTokenProvider provider,
        NavigationManager navigationManager,
        ClientConfiguration clientConfiguration)
        : base(provider, navigationManager)
    {
        // Configurar para aceitar localhost com qualquer porta (Aspire usa portas dinâmicas)
        // Usa a URL completa incluindo a porta para que Uri.IsBaseOf funcione corretamente
        ConfigureHandler(
            authorizedUrls: [clientConfiguration.ApiBaseUrl],
            scopes: ["openid", "profile", "email"]);
    }
}
