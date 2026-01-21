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
        var apiUri = new Uri(clientConfiguration.ApiBaseUrl);
        var baseUrl = $"{apiUri.Scheme}://{apiUri.Host}";
        
        ConfigureHandler(
            authorizedUrls: [baseUrl],
            scopes: ["openid", "profile", "email"]);
    }
}
