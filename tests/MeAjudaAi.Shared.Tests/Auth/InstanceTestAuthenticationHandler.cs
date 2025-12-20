using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.Shared.Tests.Auth;

/// <summary>
/// Instance-based authentication handler for tests that eliminates static state issues
/// Each test factory gets its own isolated authentication configuration
/// </summary>
public class InstanceTestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ITestAuthenticationConfiguration configuration) : BaseTestAuthenticationHandler(options, logger, encoder)
{
    public const string SchemeName = "TestInstance";

    private readonly ITestAuthenticationConfiguration _configuration = configuration;

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // If no user is configured and unauthenticated users aren't allowed, fail authentication
        if (!_configuration.HasUser && !_configuration.AllowUnauthenticated)
        {
            return Task.FromResult(AuthenticateResult.Fail("No authentication configuration set"));
        }

        // Auto-configure as admin if no configuration and unauthenticated users are allowed
        if (!_configuration.HasUser && _configuration.AllowUnauthenticated)
        {
            _configuration.ConfigureAdmin();
        }

        return Task.FromResult(CreateSuccessResult());
    }

    protected override string GetTestUserId() => _configuration.UserId ?? base.GetTestUserId();
    protected override string GetTestUserName() => _configuration.UserName ?? base.GetTestUserName();
    protected override string GetTestUserEmail() => _configuration.Email ?? base.GetTestUserEmail();
    protected override string[] GetTestUserRoles() => _configuration.Roles?.ToArray() ?? base.GetTestUserRoles();
    protected override string GetAuthenticationScheme() => SchemeName;
}
