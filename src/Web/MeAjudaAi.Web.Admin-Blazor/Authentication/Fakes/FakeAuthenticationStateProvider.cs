using System.Security.Claims;
using MeAjudaAi.Web.Admin.Authorization;
using Microsoft.AspNetCore.Components.Authorization;

namespace MeAjudaAi.Web.Admin.Authentication.Fakes;

/// <summary>
/// Provedor de estado de autenticação fake para desenvolvimento local.
/// </summary>
public class FakeAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly AuthenticationState _authState;

    public FakeAuthenticationStateProvider()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "Usuário Teste"),
            new(ClaimTypes.Email, "teste@meajudaai.com.br"),
            new("sub", "00000000-0000-0000-0000-000000000000"), // Default fixed UUID for dev
            new(ClaimTypes.Role, RoleNames.Admin),
            new(ClaimTypes.Role, RoleNames.ProviderManager),
            new("role", RoleNames.Admin), // Keycloak style
            new("role", RoleNames.ProviderManager) // Keycloak style
        };

        var identity = new ClaimsIdentity(claims, "Fake Authentication");
        var user = new ClaimsPrincipal(identity);
        _authState = new AuthenticationState(user);
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(_authState);
    }
}
