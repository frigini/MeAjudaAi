using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MeAjudaAi.Contracts.Configuration;

namespace MeAjudaAi.Web.Admin.Services;

/// <summary>
/// Handler customizado para anexar token de autenticação nas requisições à API.
/// </summary>
public class ApiAuthorizationMessageHandler : AuthorizationMessageHandler
{
    public ApiAuthorizationMessageHandler(IAccessTokenProvider provider,
        NavigationManager navigationManager,
        ClientConfiguration clientConfiguration)
        : base(provider, navigationManager)
    {
        ConfigureHandler(
            authorizedUrls: new[] { clientConfiguration.ApiBaseUrl },
            scopes: new[] { "openid", "profile", "email" });
    }
}
