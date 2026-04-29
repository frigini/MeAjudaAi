using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MeAjudaAi.ApiService.Handlers;
using MeAjudaAi.ApiService.Middlewares;
using MeAjudaAi.ApiService.Options;
using MeAjudaAi.ApiService.Services.HostedServices;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Threading.RateLimiting;

namespace MeAjudaAi.ApiService.Extensions;

/// <summary>
/// Métodos de extensão para configuração de segurança incluindo autenticação, autorização e CORS.
/// </summary>
[ExcludeFromCodeCoverage]
public static class SecurityExtensions
{
    /// <summary>
    /// Configura autenticação baseada no ambiente (Keycloak para produção, teste simples para desenvolvimento)
    /// </summary>
    public static IServiceCollection AddEnvironmentAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);
        // A autenticação específica por ambiente agora é gerenciada pelo EnvironmentSpecificExtensions
        // Aqui apenas configuramos Keycloak para ambientes não-testing
        if (!MeAjudaAi.Shared.Utilities.EnvironmentHelpers.IsSecurityBypassEnvironment(environment))
        {
            services.AddKeycloakAuthentication(configuration, environment);
        }
        else
        {
            // Em ambientes de bypass (dev/test/CI) sem Keycloak, registramos um handler no-op
            // para evitar que UseAuthentication() falhe ao tentar resolver o esquema padrão.
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            // Sem operação — ignora a lógica real de autenticação
                            return Task.CompletedTask;
                        }
                    };
                });
        }

        return services;
    }

    /// <summary>
    /// Configura autenticação JWT com Keycloak
    /// </summary>
    public static IServiceCollection AddKeycloakAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);
        // Registra KeycloakOptions usando AddOptions<>()
        services.AddOptions<KeycloakOptions>()
            .Configure<IConfiguration>((opts, config) =>
            {
                config.GetSection(KeycloakOptions.SectionName).Bind(opts);
            })
            .ValidateOnStart();

        // Obtém KeycloakOptions para uso imediato na configuração
        var keycloakOptions = configuration.GetSection(KeycloakOptions.SectionName).Get<KeycloakOptions>() ?? new KeycloakOptions();

        // Valida configuração do Keycloak
        ValidateKeycloakOptions(keycloakOptions);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.Authority = keycloakOptions.AuthorityUrl;
                options.Audience = keycloakOptions.ClientId;
                options.RequireHttpsMetadata = keycloakOptions.RequireHttpsMetadata;

                // Parâmetros aprimorados de validação do token
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = keycloakOptions.ValidateIssuer,
                    ValidateAudience = keycloakOptions.ValidateAudience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = keycloakOptions.ClockSkew,
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = "preferred_username" // Claim de usuário preferencial do Keycloak
                };

                // Adiciona eventos para log de problemas de autenticação
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerHandler>>();
                        logger.LogWarning("JWT authentication failed: {Exception}", context.Exception.Message);

                        var env = context.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
                        // Emite header de debug apenas em ambientes não-produção
                        if (!env.IsProduction())
                        {
                            var sanitizedMessage = $"{context.Exception.GetType().Name}: {context.Exception.Message.Length} chars";
                            // Mensagem completa apenas em desenvolvimento para facilitar debugging
                            if (env.IsDevelopment())
                            {
                                sanitizedMessage = $"{context.Exception.GetType().Name}: {context.Exception.Message.Replace(Environment.NewLine, " ")}";
                            }
                            context.Response.Headers.Append(AuthConstants.Headers.DebugAuthFailure, sanitizedMessage);
                        }

                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerHandler>>();
                        logger.LogInformation("JWT authentication challenge: {Error} - {ErrorDescription}",
                            context.Error, context.ErrorDescription);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerHandler>>();
                        var principal = context.Principal!;
                        var clientId = context.HttpContext.RequestServices.GetRequiredService<IOptions<KeycloakOptions>>().Value.ClientId;

                        // Copia claims existentes e adiciona roles do Keycloak
                        var claims = principal.Claims.ToList();

                        if (context.SecurityToken is JwtSecurityToken jwtToken)
                        {
                            var keycloakRoles = ExtractKeycloakRoles(jwtToken, clientId);
                            claims.AddRange(keycloakRoles);
                        }

                        var identity = new ClaimsIdentity(claims, principal.Identity?.AuthenticationType, "preferred_username", ClaimTypes.Role);
                        context.Principal = new ClaimsPrincipal(identity);

                        var userId = context.Principal.FindFirst("sub")?.Value;
                        logger.LogDebug("JWT token validated successfully for user: {UserId}", userId);
                        return Task.CompletedTask;
                    }
                };
            });

        // Registra serviço de logging de inicialização para configuração do Keycloak
        services.AddHostedService<KeycloakConfigurationLogger>();

        return services;
    }

    /// <summary>
    /// Configura políticas de autorização baseadas em permissões type-safe
    /// </summary>
    public static IServiceCollection AddAuthorizationPolicies(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment? environment = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Sistema de permissões type-safe (único e centralizado) e Adiciona resolução de permissões do Keycloak internamente
        services.AddPermissionBasedAuthorization(configuration, environment);

        // Adiciona políticas especiais que precisam de handlers customizados
        services.AddAuthorizationBuilder()
            .AddPolicy("SelfOrAdmin", policy =>
                policy.AddRequirements(new SelfOrAdminRequirement()))
            .AddPolicy("AdminOnly", policy =>
                policy.RequireRole(RoleConstants.AdminEquivalentRoles))
            .AddPolicy("SuperAdminOnly", policy =>
                policy.RequireRole(RoleConstants.SuperAdminEquivalentRoles));

        // Registra handlers de autorização customizados
        services.AddScoped<IAuthorizationHandler, SelfOrAdminHandler>();

        return services;
    }

    /// <summary>
    /// Extrai roles do token JWT do Keycloak a partir das estruturas realm_access e resource_access.
    /// </summary>
    /// <param name="jwtToken">Token JWT do Keycloak</param>
    /// <param name="clientId">ID do cliente para extração de roles específicos do cliente</param>
    /// <returns>Lista de claims de role extraídos do token</returns>
    private static List<Claim> ExtractKeycloakRoles(JwtSecurityToken jwtToken, string clientId)
    {
        var roleClaims = new List<Claim>();

        // 1. Extrai roles da claim raiz 'roles' (suportada por mappers customizados como o realm-roles)
        if (jwtToken.Payload.TryGetValue("roles", out var rootRolesObj) &&
            rootRolesObj is IEnumerable<object> rootRolesList)
        {
            foreach (var role in rootRolesList.OfType<string>())
            {
                if (!roleClaims.Any(c => c.Value == role))
                    roleClaims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        // 2. Extrai roles do realm_access (padrão do Keycloak)
        if (jwtToken.Payload.TryGetValue("realm_access", out var realmObj) &&
            realmObj is IDictionary<string, object> realmDict &&
            realmDict.TryGetValue("roles", out var realmRoles) &&
            realmRoles is IEnumerable<object> realmRolesList)
        {
            foreach (var role in realmRolesList.OfType<string>())
            {
                if (!roleClaims.Any(c => c.Value == role))
                    roleClaims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        // 3. Extrai roles do resource_access para o cliente específico
        if (jwtToken.Payload.TryGetValue("resource_access", out var resourceObj) &&
            resourceObj is IDictionary<string, object> resourceDict &&
            resourceDict.TryGetValue(clientId, out var clientObj) &&
            clientObj is IDictionary<string, object> clientDict &&
            clientDict.TryGetValue("roles", out var clientRoles) &&
            clientRoles is IEnumerable<object> clientRolesList)
        {
            foreach (var role in clientRolesList.OfType<string>())
            {
                if (!roleClaims.Any(c => c.Value == role))
                    roleClaims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        return roleClaims;
    }

    public static IServiceCollection AddCustomAntiforgery(this IServiceCollection services)
    {
        services.AddAntiforgery(options =>
        {
            // O token é enviado via header (comum em APIs/SPAs)
            options.HeaderName = "X-XSRF-TOKEN";
            options.Cookie.Name = "XSRF-TOKEN";
            options.Cookie.HttpOnly = false; // Deve ser acessível pelo JS para ler e enviar no header
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });

        return services;
    }

    /// <summary>
    /// Valida as configurações do Keycloak para garantir que estão completas.
    /// </summary>
    /// <param name="options">Opções de configuração do Keycloak</param>
    /// <exception cref="InvalidOperationException">Lançada quando configuração obrigatória está ausente</exception>
    private static void ValidateKeycloakOptions(KeycloakOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.BaseUrl))
            throw new InvalidOperationException("Keycloak BaseUrl is required but not configured");

        if (string.IsNullOrWhiteSpace(options.Realm))
            throw new InvalidOperationException("Keycloak Realm is required but not configured");

        if (string.IsNullOrWhiteSpace(options.ClientId))
            throw new InvalidOperationException("Keycloak ClientId is required but not configured");

        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _))
            throw new InvalidOperationException($"Keycloak BaseUrl '{options.BaseUrl}' is not a valid URL");

        if (options.ClockSkew.TotalMinutes > 30)
            throw new InvalidOperationException("Keycloak ClockSkew should not exceed 30 minutes for security reasons");
    }
}
