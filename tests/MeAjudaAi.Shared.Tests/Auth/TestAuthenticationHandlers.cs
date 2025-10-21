using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MeAjudaAi.Shared.Authorization;

namespace MeAjudaAi.Shared.Tests.Auth;

/// <summary>
/// Base authentication handler para testes com funcionalidades configuráveis
/// </summary>
public abstract class BaseTestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected virtual string GetTestUserId() => "test-user-id";
    protected virtual string GetTestUserName() => "test-user";
    protected virtual string GetTestUserEmail() => "test@example.com";
    protected virtual string[] GetTestUserRoles() => ["admin"];
    protected virtual string GetAuthenticationScheme() => "Test";

    protected virtual Claim[] CreateStandardClaims()
    {
        var userId = GetTestUserId();
        var userName = GetTestUserName();
        var userEmail = GetTestUserEmail();
        var roles = GetTestUserRoles();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId, ClaimValueTypes.String),
            new("sub", userId, ClaimValueTypes.String),
            new(ClaimTypes.Name, userName, ClaimValueTypes.String),
            new(ClaimTypes.Email, userEmail, ClaimValueTypes.Email),
            new("auth_time", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer),
            new("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer),
            new("exp", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role, ClaimValueTypes.String));
            claims.Add(new Claim("roles", role.ToLowerInvariant(), ClaimValueTypes.String));
            
            // Add permissions based on role for test environment
            if (role.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                // Add all admin permissions
                claims.Add(new Claim(CustomClaimTypes.Permission, Permission.UsersList.GetValue()));
                claims.Add(new Claim(CustomClaimTypes.Permission, Permission.UsersRead.GetValue()));
                claims.Add(new Claim(CustomClaimTypes.Permission, Permission.UsersCreate.GetValue()));
                claims.Add(new Claim(CustomClaimTypes.Permission, Permission.UsersUpdate.GetValue()));
                claims.Add(new Claim(CustomClaimTypes.Permission, Permission.UsersDelete.GetValue()));
                claims.Add(new Claim(CustomClaimTypes.Permission, Permission.AdminUsers.GetValue()));
                claims.Add(new Claim(CustomClaimTypes.IsSystemAdmin, "true"));
            }
            else if (role.Equals("user", StringComparison.OrdinalIgnoreCase))
            {
                // Add basic user permissions
                claims.Add(new Claim(CustomClaimTypes.Permission, Permission.UsersProfile.GetValue()));
                claims.Add(new Claim(CustomClaimTypes.Permission, Permission.UsersRead.GetValue()));
            }
        }

        return [.. claims];
    }

    protected virtual AuthenticateResult CreateSuccessResult()
    {
        var claims = CreateStandardClaims();
        var identity = new ClaimsIdentity(claims, GetAuthenticationScheme(), ClaimTypes.Name, ClaimTypes.Role);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, GetAuthenticationScheme());

        return AuthenticateResult.Success(ticket);
    }
}
