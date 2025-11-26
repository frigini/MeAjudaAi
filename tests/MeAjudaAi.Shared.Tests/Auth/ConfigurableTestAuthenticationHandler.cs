using System.Collections.Concurrent;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.Shared.Tests.Auth;

/// <summary>
/// Authentication handler configurável para testes específicos
/// Permite configurar usuário, roles e comportamento dinamicamente
/// Usa contexto HTTP para isolamento thread-safe em testes paralelos
/// </summary>
public class ConfigurableTestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : BaseTestAuthenticationHandler(options, logger, encoder)
{
    public const string SchemeName = "TestConfigurable";
    private const string TestContextHeader = "X-Test-Context-Id";

    // Thread-safe dictionary indexed by test context ID
    private static readonly ConcurrentDictionary<string, UserConfig> _userConfigs = new();
    private static readonly ConcurrentDictionary<string, bool> _allowUnauthenticatedByContext = new();

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Get test context ID from header
        var contextId = GetTestContextId();
        
        // Authentication must be explicitly configured via ConfigureUser/ConfigureAdmin/etc.
        if (contextId == null || !_userConfigs.TryGetValue(contextId, out _))
        {
            // If allowUnauthenticated is true for this context, succeed with an anonymous principal
            if (contextId != null && _allowUnauthenticatedByContext.TryGetValue(contextId, out var allowUnauth) && allowUnauth)
            {
                // Return success with an empty identity (no claims, no roles, no permissions)
                // This represents a truly anonymous/unauthenticated user
                var anonymousIdentity = new System.Security.Claims.ClaimsIdentity(
                    authenticationType: SchemeName,
                    nameType: null,
                    roleType: null);
                var anonymousPrincipal = new System.Security.Claims.ClaimsPrincipal(anonymousIdentity);
                var ticket = new AuthenticationTicket(anonymousPrincipal, SchemeName);
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }

            return Task.FromResult(AuthenticateResult.Fail("No authentication configuration set"));
        }

        return Task.FromResult(CreateSuccessResult());
    }

    private string? GetTestContextId()
    {
        // Try to get context ID from request header
        if (Context.Request.Headers.TryGetValue(TestContextHeader, out var headerValue))
        {
            return headerValue.ToString();
        }

        return null;
    }

    protected override string GetTestUserId()
    {
        var contextId = GetTestContextId();
        return contextId != null && _userConfigs.TryGetValue(contextId, out var config)
            ? config.UserId : base.GetTestUserId();
    }

    protected override string GetTestUserName()
    {
        var contextId = GetTestContextId();
        return contextId != null && _userConfigs.TryGetValue(contextId, out var config)
            ? config.UserName : base.GetTestUserName();
    }

    protected override string GetTestUserEmail()
    {
        var contextId = GetTestContextId();
        return contextId != null && _userConfigs.TryGetValue(contextId, out var config)
            ? config.Email : base.GetTestUserEmail();
    }

    protected override string[] GetTestUserRoles()
    {
        var contextId = GetTestContextId();
        return contextId != null && _userConfigs.TryGetValue(contextId, out var config)
            ? config.Roles : base.GetTestUserRoles();
    }

    protected override System.Security.Claims.Claim[] CreateStandardClaims()
    {
        var baseClaims = base.CreateStandardClaims().ToList();
        var contextId = GetTestContextId();

        if (contextId != null && _userConfigs.TryGetValue(contextId, out var config))
        {
            // Override permissions only when explicitly provided
            if (config.Permissions is { Length: > 0 })
            {
                baseClaims.RemoveAll(c => c.Type == Authorization.CustomClaimTypes.Permission);
                foreach (var permission in config.Permissions)
                {
                    baseClaims.Add(new System.Security.Claims.Claim(Authorization.CustomClaimTypes.Permission, permission));
                }
            }

            // Always align IsSystemAdmin claim with config when a user config is present
            baseClaims.RemoveAll(c => c.Type == Authorization.CustomClaimTypes.IsSystemAdmin);
            if (config.IsSystemAdmin)
            {
                baseClaims.Add(new System.Security.Claims.Claim(Authorization.CustomClaimTypes.IsSystemAdmin, "true"));
            }
        }

        return [.. baseClaims];
    }

    protected override string GetAuthenticationScheme() => SchemeName;

    // Thread-local storage for test context ID
    private static readonly AsyncLocal<string?> _currentTestContextId = new();

    public static string GetOrCreateTestContext()
    {
        if (_currentTestContextId.Value == null)
        {
            _currentTestContextId.Value = Guid.NewGuid().ToString();
        }
        return _currentTestContextId.Value;
    }

    public static void ConfigureUser(string userId, string userName, string email, string[] permissions, bool isSystemAdmin = false, params string[] roles)
    {
        var contextId = GetOrCreateTestContext();
        _userConfigs[contextId] = new UserConfig(
            userId, 
            userName, 
            email, 
            roles.Length > 0 ? roles : ["user"], 
            permissions, 
            isSystemAdmin);
    }

    public static void ConfigureUserWithRoles(string userId, string userName, string email, params string[] roles)
    {
        var contextId = GetOrCreateTestContext();
        _userConfigs[contextId] = new UserConfig(userId, userName, email, roles, [], false);
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
        var contextId = _currentTestContextId.Value;
        if (contextId != null)
        {
            _userConfigs.TryRemove(contextId, out _);
            _allowUnauthenticatedByContext.TryRemove(contextId, out _);
        }
        _currentTestContextId.Value = null;
    }

    public static void SetAllowUnauthenticated(bool allow)
    {
        var contextId = GetOrCreateTestContext();
        _allowUnauthenticatedByContext[contextId] = allow;
    }

    // Add method for better debugging in tests
    public static bool HasConfiguration()
    {
        var contextId = _currentTestContextId.Value;
        return contextId != null && _userConfigs.ContainsKey(contextId);
    }

    public static bool GetAllowUnauthenticated()
    {
        var contextId = _currentTestContextId.Value;
        return contextId != null && 
               _allowUnauthenticatedByContext.TryGetValue(contextId, out var allow) && 
               allow;
    }

    public static string? GetCurrentTestContextId() => _currentTestContextId.Value;

    private record UserConfig(string UserId, string UserName, string Email, string[] Roles, string[] Permissions, bool IsSystemAdmin);
}
