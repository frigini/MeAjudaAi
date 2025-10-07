using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace MeAjudaAi.Shared.Tests.Auth;

/// <summary>
/// Authentication handler para testes Aspire que verifica Authorization headers
/// Autentica se header presente, falha se ausente (usuário anônimo)
/// </summary>
public class AspireTestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : BaseTestAuthenticationHandler(options, logger, encoder)
{
    public const string SchemeName = "AspireTest";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers.Authorization.FirstOrDefault();

        if (string.IsNullOrEmpty(authHeader))
        {
            Logger.LogDebug("Aspire test: No authorization header - anonymous user");
            return Task.FromResult(AuthenticateResult.Fail("No authorization header"));
        }

        Logger.LogDebug("Aspire test: Authorization header present - authenticated as admin");
        return Task.FromResult(CreateSuccessResult());
    }

    protected override string GetTestUserId() => "aspire-test-user-id";
    protected override string GetTestUserName() => "aspire-test-user";
    protected override string GetTestUserEmail() => "aspire-test@example.com";
    protected override string GetAuthenticationScheme() => SchemeName;
}
