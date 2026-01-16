using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace MeAjudaAi.Web.Admin.Services;

/// <summary>
/// Handler customizado para anexar token de autenticação nas requisições à API.
/// </summary>
public class ApiAuthorizationMessageHandler : AuthorizationMessageHandler
{
    public ApiAuthorizationMessageHandler(IAccessTokenProvider provider,
        NavigationManager navigationManager,
        IConfiguration configuration)
        : base(provider, navigationManager)
    {
        var apiBaseUrl = configuration["ApiBaseUrl"] 
            ?? navigationManager.BaseUri;

        ConfigureHandler(
            authorizedUrls: new[] { apiBaseUrl },
            scopes: new[] { "openid", "profile", "email" });
    }
}
