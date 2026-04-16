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
    /// Nome do esquema de autenticação usado pelo handler de teste configurável.
    /// </summary>
    /// <remarks>
    /// Este nome de esquema é registrado com os serviços de autenticação do ASP.NET Core durante a configuração de testes.
    /// Ele identifica o handler de autenticação que processa as requisições de autenticação de teste.
    /// </remarks>
    /// <seealso cref="GetAuthenticationScheme"/>
    /// <seealso cref="TestContextHeader"/>
    public const string SchemeName = "TestConfigurable";

    /// <summary>
    /// Nome do cabeçalho HTTP usado para transmitir o ID de contexto de teste para isolamento de autenticação por teste.
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

            // Alinha a claim IsSystemAdmin com a config quando presente
            baseClaims.RemoveAll(c => c.Type == AuthConstants.Claims.IsSystemAdmin);
            if (config.IsSystemAdmin)
            {
                baseClaims.Add(new System.Security.Claims.Claim(AuthConstants.Claims.IsSystemAdmin, "true"));
            }

            // Adiciona claim provider_id se presente
            if (config.ProviderId.HasValue)
            {
                var pIdValue = config.ProviderId.Value.ToString();
                baseClaims.RemoveAll(c => c.Type == AuthConstants.Claims.ProviderId);
                baseClaims.Add(new System.Security.Claims.Claim(AuthConstants.Claims.ProviderId, pIdValue));
            }
        }

        return [.. baseClaims];
    }

    protected override string GetAuthenticationScheme() => SchemeName;

    // Armazenamento local de thread para o ID de contexto do teste
    private static readonly AsyncLocal<string?> _currentTestContextId = new();

    /// <summary>
    /// Obtém ou cria um ID de contexto de teste para o fluxo assíncrono atual.
    /// Cada teste recebe seu próprio contexto isolado para execução paralela thread-safe.
    /// </summary>
    /// <returns>O ID do contexto de teste</returns>
    public static string GetOrCreateTestContext()
    {
        if (_currentTestContextId.Value == null)
        {
            _currentTestContextId.Value = UuidGenerator.NewId().ToString();
        }
        return _currentTestContextId.Value;
    }

    /// <summary>
    /// Configura um usuário de teste com permissões e roles específicas.
    /// </summary>
    /// <param name="userId">O ID do usuário</param>
    /// <param name="userName">O nome de usuário</param>
    /// <param name="email">O email do usuário</param>
    /// <param name="permissions">Array de permissões concedidas</param>
    /// <param name="isSystemAdmin">Indica se o usuário é um administrador do sistema</param>
    /// <param name="roles">Array de roles atribuídas</param>
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
    /// Configura um usuário de teste apenas com roles (sem permissões explícitas).
    /// As permissões serão derivadas das roles com base na lógica do handler base.
    /// </summary>
    /// <param name="userId">O ID do usuário</param>
    /// <param name="userName">O nome de usuário</param>
    /// <param name="email">O email do usuário</param>
    /// <param name="roles">Array de roles atribuídas</param>
    public static void ConfigureUserWithRoles(string userId, string userName, string email, params string[] roles)
    {
        var contextId = GetOrCreateTestContext();
        _userConfigs[contextId] = new UserConfig(userId, userName, email, roles, [], false);
    }

    /// <summary>
    /// Configura um usuário administrador com privilégios totais de administrador do sistema.
    /// </summary>
    /// <param name="userId">O ID do usuário</param>
    /// <param name="userName">O nome de usuário</param>
    /// <param name="email">O email do usuário</param>
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
    /// Configura um usuário comum (não-admin) com permissões básicas.
    /// </summary>
    /// <param name="userId">O ID do usuário</param>
    /// <param name="userName">O nome de usuário</param>
    /// <param name="email">O email do usuário</param>
    public static void ConfigureRegularUser(string userId = "user-id", string username = "user", string email = "user@test.com")
    {
        ConfigureUserWithRoles(userId, username, email, "user");
    }

    /// <summary>
    /// Configura um usuário prestador com um ID de prestador específico.
    /// </summary>
    public static void ConfigureProvider(string userId = "provider-id", string userName = "provider", Guid? providerId = null, string email = "provider@test.com")
    {
        var contextId = GetOrCreateTestContext();
        _userConfigs[contextId] = new UserConfig(
            userId,
            userName,
            email,
            ["provider"],
            [],
            false,
            providerId);
    }

    /// <summary>
    /// Limpa a configuração de autenticação para o contexto de teste atual.
    /// Deve ser chamado ao final de cada teste para evitar poluição de estado.
    /// Recomendado: invocar a partir de ganchos de limpeza de teste (ex.: xUnit IAsyncLifetime.DisposeAsync).
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
    /// Define se requisições não autenticadas devem ser permitidas para o contexto de teste atual.
    /// Quando verdadeiro, as requisições sucedem com um principal anônimo (sem claims/roles/permissões).
    /// </summary>
    /// <param name="allow">True para permitir requisições não autenticadas, false para exigir autenticação</param>
    public static void SetAllowUnauthenticated(bool allow)
    {
        var contextId = GetOrCreateTestContext();
        _allowUnauthenticatedByContext[contextId] = allow;
    }

    /// <summary>
    /// Verifica se a autenticação está configurada para o contexto de teste atual.
    /// </summary>
    /// <returns>True se existir uma configuração de usuário para o contexto atual</returns>
    public static bool HasConfiguration()
    {
        var contextId = _currentTestContextId.Value;
        return contextId != null && _userConfigs.ContainsKey(contextId);
    }

    /// <summary>
    /// Obtém a configuração atual de allowUnauthenticated para o contexto de teste atual.
    /// </summary>
    /// <returns>True se requisições não autenticadas são permitidas, false caso contrário</returns>
    public static bool GetAllowUnauthenticated()
    {
        var contextId = _currentTestContextId.Value;
        return contextId != null &&
               _allowUnauthenticatedByContext.TryGetValue(contextId, out var allow) &&
               allow;
    }

    /// <summary>
    /// Obtém o ID do contexto de teste atual para fins de depuração.
    /// </summary>
    /// <returns>O ID do contexto de teste atual, ou null se não inicializado</returns>
    public static string? GetCurrentTestContextId() => _currentTestContextId.Value;

    private record UserConfig(string UserId, string UserName, string Email, string[] Roles, string[] Permissions, bool IsSystemAdmin, Guid? ProviderId = null);
}
