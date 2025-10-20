using System.Collections.Concurrent;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.Shared.Tests.Auth;

/// <summary>
/// Authentication handler configurável para testes específicos
/// Permite configurar usuário, roles e comportamento dinamicamente
/// </summary>
public class ConfigurableTestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : BaseTestAuthenticationHandler(options, logger, encoder)
{
    public const string SchemeName = "TestConfigurable";

    private static readonly ConcurrentDictionary<string, UserConfig> _userConfigs = new();
    private static volatile string? _currentConfigKey;

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (_currentConfigKey == null || !_userConfigs.TryGetValue(_currentConfigKey, out _))
        {
            return Task.FromResult(AuthenticateResult.Fail("No test user configured"));
        }

        return Task.FromResult(CreateSuccessResult());
    }

    protected override string GetTestUserId() =>
        _currentConfigKey != null && _userConfigs.TryGetValue(_currentConfigKey, out var config)
            ? config.UserId : base.GetTestUserId();

    protected override string GetTestUserName() =>
        _currentConfigKey != null && _userConfigs.TryGetValue(_currentConfigKey, out var config)
            ? config.UserName : base.GetTestUserName();

    protected override string GetTestUserEmail() =>
        _currentConfigKey != null && _userConfigs.TryGetValue(_currentConfigKey, out var config)
            ? config.Email : base.GetTestUserEmail();

    protected override string[] GetTestUserRoles() =>
        _currentConfigKey != null && _userConfigs.TryGetValue(_currentConfigKey, out var config)
            ? config.Roles : base.GetTestUserRoles();

    protected override string GetAuthenticationScheme() => SchemeName;

    public static void ConfigureUser(string userId, string userName, string email, params string[] roles)
    {
        var key = $"{userId}_{userName}";
        _userConfigs[key] = new UserConfig(userId, userName, email, roles);
        _currentConfigKey = key;
    }

    public static void ConfigureAdmin(string userId = "admin-id", string userName = "admin", string email = "admin@test.com")
    {
        ConfigureUser(userId, userName, email, "admin");
    }

    public static void ConfigureRegularUser(string userId = "user-id", string userName = "user", string email = "user@test.com")
    {
        ConfigureUser(userId, userName, email, "user");
    }

    public static void ClearConfiguration()
    {
        _userConfigs.Clear();
        _currentConfigKey = null;
    }

    private record UserConfig(string UserId, string UserName, string Email, string[] Roles);
}
