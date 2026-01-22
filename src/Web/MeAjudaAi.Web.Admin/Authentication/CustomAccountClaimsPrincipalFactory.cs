using System.Security.Claims;
using System.Text.Json;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;

namespace MeAjudaAi.Web.Admin.Authentication;

/// <summary>
/// Factory customizado para processar claims do Keycloak, especialmente o array de roles.
/// Keycloak retorna roles como um array JSON ["role1", "role2"], mas o Blazor precisa de claims individuais.
/// </summary>
public class CustomAccountClaimsPrincipalFactory : AccountClaimsPrincipalFactory<RemoteUserAccount>
{
    public CustomAccountClaimsPrincipalFactory(IAccessTokenProviderAccessor accessor)
        : base(accessor)
    {
    }

    public override async ValueTask<ClaimsPrincipal> CreateUserAsync(
        RemoteUserAccount account,
        RemoteAuthenticationUserOptions options)
    {
        var user = await base.CreateUserAsync(account, options);

        if (user.Identity is ClaimsIdentity identity)
        {
            // Configurar o RoleClaimType para que IsInRole() funcione corretamente
            var newIdentity = new ClaimsIdentity(
                identity.Claims,
                identity.AuthenticationType,
                identity.NameClaimType,
                ClaimTypes.Role); // Define explicitamente o tipo de claim para roles

            // Procurar todos os claims de roles (pode ser "roles" ou "role")
            var rolesClaims = newIdentity.FindAll(c => c.Type == AuthConstants.Claims.Roles || c.Type == "role").ToList();

            foreach (var rolesClaim in rolesClaims)
            {
                // Remover o claim original de roles
                newIdentity.RemoveClaim(rolesClaim);

                var rolesValue = rolesClaim.Value;

                // Se for um array JSON (começa com [), fazer parse
                if (rolesValue.StartsWith('['))
                {
                    try
                    {
                        var roles = JsonSerializer.Deserialize<string[]>(rolesValue);
                        if (roles != null)
                        {
                            foreach (var role in roles)
                            {
                                // Adicionar cada role como um claim individual do tipo "role"
                                // ClaimTypes.Role = http://schemas.microsoft.com/ws/2008/06/identity/claims/role
                                if (!string.IsNullOrWhiteSpace(role))
                                {
                                    newIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
                                }
                            }
                        }
                    }
                    catch (JsonException)
                    {
                        // Se falhar o parse, adicionar como está
                        newIdentity.AddClaim(new Claim(ClaimTypes.Role, rolesValue));
                    }
                }
                else
                {
                    // Se não for array, adicionar diretamente
                    newIdentity.AddClaim(new Claim(ClaimTypes.Role, rolesValue));
                }
            }

            return new ClaimsPrincipal(newIdentity);
        }

        return user;
    }
}
