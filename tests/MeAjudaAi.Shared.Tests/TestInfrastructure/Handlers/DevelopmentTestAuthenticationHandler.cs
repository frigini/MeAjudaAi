using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers;

/// <summary>
/// Authentication handler para desenvolvimento que SEMPRE autentica como admin
/// ⚠️ NUNCA USAR EM PRODUÇÃO ⚠️
/// </summary>
public class DevelopmentTestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : BaseTestAuthenticationHandler(options, logger, encoder)
{
    public const string SchemeName = "DevelopmentTest";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Logger.LogWarning("🚨 DEVELOPMENT TEST AUTHENTICATION: Always authenticating as admin. NEVER use in production!");
        return Task.FromResult(CreateSuccessResult());
    }

    protected override string GetAuthenticationScheme() => SchemeName;
}
