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
    protected override string[] GetTestUserRoles() => _configuration.Roles ?? base.GetTestUserRoles();
    protected override string GetAuthenticationScheme() => SchemeName;
}

/// <summary>
/// Interface for test authentication configuration that allows instance-based configuration
/// </summary>
public interface ITestAuthenticationConfiguration
{
    string? UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    string[]? Roles { get; }
    bool HasUser { get; }
    bool AllowUnauthenticated { get; }
    
    void ConfigureUser(string userId, string userName, string email, params string[] roles);
    void ConfigureAdmin(string userId = "admin-id", string userName = "admin", string email = "admin@test.com");
    void ConfigureRegularUser(string userId = "user-id", string userName = "user", string email = "user@test.com");
    void ClearConfiguration();
    void SetAllowUnauthenticated(bool allow);
}

/// <summary>
/// Implementation of test authentication configuration with instance-based state
/// </summary>
public class TestAuthenticationConfiguration : ITestAuthenticationConfiguration
{
    public string? UserId { get; private set; }
    public string? UserName { get; private set; }
    public string? Email { get; private set; }
    public string[]? Roles { get; private set; }
    public bool HasUser => UserId != null;
    public bool AllowUnauthenticated { get; private set; }

    public void ConfigureUser(string userId, string userName, string email, params string[] roles)
    {
        UserId = userId;
        UserName = userName;
        Email = email;
        Roles = roles;
    }

    public void ConfigureAdmin(string userId = "admin-id", string userName = "admin", string email = "admin@test.com")
    {
        ConfigureUser(userId, userName, email, "admin");
    }

    public void ConfigureRegularUser(string userId = "user-id", string userName = "user", string email = "user@test.com")
    {
        ConfigureUser(userId, userName, email, "user");
    }

    public void ClearConfiguration()
    {
        UserId = null;
        UserName = null;
        Email = null;
        Roles = null;
        AllowUnauthenticated = false;
    }

    public void SetAllowUnauthenticated(bool allow)
    {
        AllowUnauthenticated = allow;
    }
}