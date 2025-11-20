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
    private static volatile bool _allowUnauthenticated = false;

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Se não há configuração e não permitimos usuários não autenticados, falha a autenticação
        if (_currentConfigKey == null || !_userConfigs.TryGetValue(_currentConfigKey, out _))
        {
            if (!_allowUnauthenticated)
            {
                return Task.FromResult(AuthenticateResult.Fail("No authentication configuration set"));
            }

            // Auto-configure como admin se não há configuração e permitimos usuários não autenticados
            ConfigureAdmin();
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

    protected override System.Security.Claims.Claim[] CreateStandardClaims()
    {
        var baseClaims = base.CreateStandardClaims().ToList();

        // Se há configuração customizada com permissions, adiciona ou substitui
        if (_currentConfigKey != null && _userConfigs.TryGetValue(_currentConfigKey, out var config) && config.Permissions.Length > 0)
        {
            // Remove as permissions geradas automaticamente pelos roles
            baseClaims.RemoveAll(c => c.Type == Authorization.CustomClaimTypes.Permission);

            // Adiciona as permissions customizadas
            foreach (var permission in config.Permissions)
            {
                baseClaims.Add(new System.Security.Claims.Claim(Authorization.CustomClaimTypes.Permission, permission));
            }

            // Se for system admin, adiciona a claim
            if (config.IsSystemAdmin)
            {
                baseClaims.RemoveAll(c => c.Type == Authorization.CustomClaimTypes.IsSystemAdmin);
                baseClaims.Add(new System.Security.Claims.Claim(Authorization.CustomClaimTypes.IsSystemAdmin, "true"));
            }
        }

        return [.. baseClaims];
    }

    protected override string GetAuthenticationScheme() => SchemeName;

    public static void ConfigureUser(string userId, string userName, string email, string[] permissions, bool isSystemAdmin = false, params string[] roles)
    {
        var key = $"{userId}_{userName}";
        _userConfigs[key] = new UserConfig(userId, userName, email, roles.Length > 0 ? roles : ["user"], permissions, isSystemAdmin);
        _currentConfigKey = key;
    }

    public static void ConfigureUserWithRoles(string userId, string userName, string email, params string[] roles)
    {
        var key = $"{userId}_{userName}";
        _userConfigs[key] = new UserConfig(userId, userName, email, roles, [], false);
        _currentConfigKey = key;
    }

    public static void ConfigureAdmin(string userId = "admin-id", string userName = "admin", string email = "admin@test.com")
    {
        ConfigureUserWithRoles(userId, userName, email, "admin");
    }

    public static void ConfigureRegularUser(string userId = "user-id", string userName = "user", string email = "user@test.com")
    {
        ConfigureUserWithRoles(userId, userName, email, "user");
    }

    public static void ClearConfiguration()
    {
        _userConfigs.Clear();
        _currentConfigKey = null;
        _allowUnauthenticated = false;  // Default to requiring authentication
    }

    public static void SetAllowUnauthenticated(bool allow)
    {
        _allowUnauthenticated = allow;
    }

    // Add method for better debugging in tests
    public static bool HasConfiguration()
    {
        return _currentConfigKey != null && _userConfigs.ContainsKey(_currentConfigKey);
    }

    public static bool GetAllowUnauthenticated()
    {
        return _allowUnauthenticated;
    }

    private record UserConfig(string UserId, string UserName, string Email, string[] Roles, string[] Permissions, bool IsSystemAdmin);
}
