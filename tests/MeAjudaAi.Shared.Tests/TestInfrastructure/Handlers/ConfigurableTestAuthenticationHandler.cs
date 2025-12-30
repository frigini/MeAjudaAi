using System.Collections.Concurrent;
using System.Text.Encodings.Web;
using MeAjudaAi.Shared.Utilities.Constants;
using MeAjudaAi.Shared.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers;

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
    /// <summary>
    /// Authentication scheme name used by the configurable test handler.
    /// </summary>
    /// <remarks>
    /// This scheme name is registered with ASP.NET Core authentication services during test setup.
    /// It identifies the authentication handler that processes test authentication requests.
    /// </remarks>
    /// <seealso cref="GetAuthenticationScheme"/>
    /// <seealso cref="TestContextHeader"/>
    public const string SchemeName = "TestConfigurable";

    /// <summary>
    /// HTTP header name used to transmit the test context ID for per-test authentication isolation.
    /// </summary>
    public const string TestContextHeader = "X-Test-Context-Id";

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

        // Fallback to AsyncLocal context ID if header is not present
        return _currentTestContextId.Value;
    }

    /// <summary>
    /// Retrieves a value from the current test context's user configuration, or returns a default value.
    /// </summary>
    /// <typeparam name="T">The type of value to retrieve</typeparam>
    /// <param name="selector">Function to extract the desired value from the user configuration</param>
    /// <param name="defaultValue">Function to provide the default value if no configuration exists</param>
    /// <returns>The configured value or the default value</returns>
    private T GetConfigValueOrDefault<T>(Func<UserConfig, T> selector, Func<T> defaultValue)
    {
        var contextId = GetTestContextId();
        return contextId != null && _userConfigs.TryGetValue(contextId, out var config)
            ? selector(config)
            : defaultValue();
    }

    protected override string GetTestUserId() =>
        GetConfigValueOrDefault(c => c.UserId, base.GetTestUserId);

    protected override string GetTestUserName() =>
        GetConfigValueOrDefault(c => c.UserName, base.GetTestUserName);

    protected override string GetTestUserEmail() =>
        GetConfigValueOrDefault(c => c.Email, base.GetTestUserEmail);

    protected override string[] GetTestUserRoles() =>
        GetConfigValueOrDefault(c => c.Roles, base.GetTestUserRoles);

    protected override System.Security.Claims.Claim[] CreateStandardClaims()
    {
        var baseClaims = base.CreateStandardClaims().ToList();
        var contextId = GetTestContextId();

        if (contextId != null && _userConfigs.TryGetValue(contextId, out var config))
        {
            // Override permissions only when explicitly provided
            if (config.Permissions is { Length: > 0 })
            {
                baseClaims.RemoveAll(c => c.Type == AuthConstants.Claims.Permission);
                foreach (var permission in config.Permissions)
                {
                    baseClaims.Add(new System.Security.Claims.Claim(AuthConstants.Claims.Permission, permission));
                }
            }

            // Always align IsSystemAdmin claim with config when a user config is present
            baseClaims.RemoveAll(c => c.Type == AuthConstants.Claims.IsSystemAdmin);
            if (config.IsSystemAdmin)
            {
                baseClaims.Add(new System.Security.Claims.Claim(AuthConstants.Claims.IsSystemAdmin, "true"));
            }
        }

        return [.. baseClaims];
    }

    protected override string GetAuthenticationScheme() => SchemeName;

    // Thread-local storage for test context ID
    private static readonly AsyncLocal<string?> _currentTestContextId = new();

    /// <summary>
    /// Gets or creates a test context ID for the current async flow.
    /// Each test gets its own isolated context for thread-safe parallel execution.
    /// </summary>
    /// <returns>The test context ID</returns>
    public static string GetOrCreateTestContext()
    {
        if (_currentTestContextId.Value == null)
        {
            _currentTestContextId.Value = UuidGenerator.NewId().ToString();
        }
        return _currentTestContextId.Value;
    }

    /// <summary>
    /// Configures a test user with specific permissions and roles.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="userName">The username</param>
    /// <param name="email">The user email</param>
    /// <param name="permissions">Array of permissions to grant</param>
    /// <param name="isSystemAdmin">Whether the user is a system administrator</param>
    /// <param name="roles">Array of roles to assign</param>
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

    /// <summary>
    /// Configures a test user with only roles (no explicit permissions).
    /// Permissions will be derived from the roles based on the base handler logic.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="userName">The username</param>
    /// <param name="email">The user email</param>
    /// <param name="roles">Array of roles to assign</param>
    public static void ConfigureUserWithRoles(string userId, string userName, string email, params string[] roles)
    {
        var contextId = GetOrCreateTestContext();
        _userConfigs[contextId] = new UserConfig(userId, userName, email, roles, [], false);
    }

    /// <summary>
    /// Configures an admin user with full system administrator privileges.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="userName">The username</param>
    /// <param name="email">The user email</param>
    public static void ConfigureAdmin(string userId = "admin-id", string userName = "admin", string email = "admin@test.com")
    {
        ConfigureUser(
            userId,
            userName,
            email,
            permissions: [],
            isSystemAdmin: true,
            roles: "admin");
    }

    /// <summary>
    /// Configures a regular (non-admin) user with basic permissions.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="userName">The username</param>
    /// <param name="email">The user email</param>
    public static void ConfigureRegularUser(string userId = "user-id", string userName = "user", string email = "user@test.com")
    {
        ConfigureUserWithRoles(userId, userName, email, "user");
    }

    /// <summary>
    /// Clears the authentication configuration for the current test context.
    /// Should be called at the end of each test to avoid state pollution.
    /// Recommended: invoke from test cleanup hooks (e.g., xUnit IAsyncLifetime.DisposeAsync) rather than ad hoc in tests.
    /// </summary>
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

    /// <summary>
    /// Sets whether unauthenticated requests should be allowed for the current test context.
    /// When true, requests succeed with an anonymous principal (no claims/roles/permissions).
    /// </summary>
    /// <param name="allow">True to allow unauthenticated requests, false to require authentication</param>
    public static void SetAllowUnauthenticated(bool allow)
    {
        var contextId = GetOrCreateTestContext();
        _allowUnauthenticatedByContext[contextId] = allow;
    }

    /// <summary>
    /// Checks if authentication is configured for the current test context.
    /// </summary>
    /// <returns>True if a user configuration exists for the current context</returns>
    public static bool HasConfiguration()
    {
        var contextId = _currentTestContextId.Value;
        return contextId != null && _userConfigs.ContainsKey(contextId);
    }

    /// <summary>
    /// Gets the current allowUnauthenticated setting for the current test context.
    /// </summary>
    /// <returns>True if unauthenticated requests are allowed, false otherwise</returns>
    public static bool GetAllowUnauthenticated()
    {
        var contextId = _currentTestContextId.Value;
        return contextId != null &&
               _allowUnauthenticatedByContext.TryGetValue(contextId, out var allow) &&
               allow;
    }

    /// <summary>
    /// Gets the current test context ID for debugging purposes.
    /// </summary>
    /// <returns>The current test context ID, or null if not initialized</returns>
    public static string? GetCurrentTestContextId() => _currentTestContextId.Value;

    private record UserConfig(string UserId, string UserName, string Email, string[] Roles, string[] Permissions, bool IsSystemAdmin);
}
