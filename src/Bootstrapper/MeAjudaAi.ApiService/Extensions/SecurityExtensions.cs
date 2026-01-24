using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MeAjudaAi.ApiService.Handlers;
using MeAjudaAi.ApiService.Options;
using MeAjudaAi.ApiService.Services.HostedServices;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Shared.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MeAjudaAi.ApiService.Extensions;

/// <summary>
/// Métodos de extensão para configuração de segurança incluindo autenticação, autorização e CORS.
/// </summary>
public static class SecurityExtensions
{
    /// <summary>
    /// Valida todas as configurações relacionadas à segurança para evitar erros em produção.
    /// </summary>
    /// <param name="configuration">Configuração da aplicação</param>
    /// <param name="environment">Ambiente de hospedagem</param>
    /// <exception cref="InvalidOperationException">Lançada quando a configuração de segurança é inválida</exception>
    public static void ValidateSecurityConfiguration(IConfiguration configuration, IWebHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        var errors = new List<string>();

        // Valida configuração de CORS
        try
        {
            var corsOptions = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();
            corsOptions.Validate();

            // Validações adicionais específicas para produção
            if (environment.IsProduction())
            {
                if (corsOptions.AllowedOrigins.Contains("*"))
                    errors.Add("Wildcard CORS origin (*) is not allowed in production environment");

                if (corsOptions.AllowedOrigins.Any(o => o.StartsWith("http://", StringComparison.OrdinalIgnoreCase)))
                    errors.Add("HTTP origins are not recommended in production - use HTTPS");

                if (corsOptions.AllowCredentials && corsOptions.AllowedOrigins.Count > 5)
                    errors.Add("Too many allowed origins with credentials enabled increases security risk");
            }
        }
        catch (InvalidOperationException ex)
        {
            errors.Add($"CORS configuration error: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            errors.Add($"CORS configuration error: {ex.Message}");
        }

        // Valida configuração do Keycloak (se não estiver em ambiente de teste)
        if (!environment.IsEnvironment("Testing"))
        {
            try
            {
                var keycloakOptions = configuration.GetSection(KeycloakOptions.SectionName).Get<KeycloakOptions>() ?? new KeycloakOptions();
                ValidateKeycloakOptions(keycloakOptions);

                // Validações adicionais específicas para produção
                if (environment.IsProduction())
                {
                    if (!keycloakOptions.RequireHttpsMetadata)
                        errors.Add("RequireHttpsMetadata must be true in production environment");

                    if (keycloakOptions.BaseUrl?.StartsWith("http://", StringComparison.OrdinalIgnoreCase) == true)
                        errors.Add("Keycloak BaseUrl must use HTTPS in production environment");

                    if (keycloakOptions.ClockSkew.TotalMinutes > 5)
                        errors.Add("Keycloak ClockSkew should be minimal (≤5 minutes) in production for higher security");
                }
            }
            catch (InvalidOperationException ex)
            {
                errors.Add($"Keycloak configuration error: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                errors.Add($"Keycloak configuration error: {ex.Message}");
            }
        }

        // Valida configuração de Rate Limiting
        try
        {
            var rateLimitSection = configuration.GetSection("AdvancedRateLimit");
            if (rateLimitSection.Exists())
            {
                var anonymousLimits = rateLimitSection.GetSection("Anonymous");
                var authenticatedLimits = rateLimitSection.GetSection("Authenticated");

                if (anonymousLimits.Exists())
                {
                    var anonMinute = anonymousLimits.GetValue<int>("RequestsPerMinute");
                    var anonHour = anonymousLimits.GetValue<int>("RequestsPerHour");

                    if (anonMinute <= 0 || anonHour <= 0)
                        errors.Add("Anonymous request limits must be positive values");

                    if (environment.IsProduction() && anonMinute > 100)
                        errors.Add("Anonymous request limits should be conservative in production (≤100 req/min)");
                }

                if (authenticatedLimits.Exists())
                {
                    var authMinute = authenticatedLimits.GetValue<int>("RequestsPerMinute");
                    var authHour = authenticatedLimits.GetValue<int>("RequestsPerHour");

                    if (authMinute <= 0 || authHour <= 0)
                        errors.Add("Authenticated request limits must be positive values");
                }
            }
        }
        catch (InvalidOperationException ex)
        {
            errors.Add($"Rate limiting configuration error: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            errors.Add($"Rate limiting configuration error: {ex.Message}");
        }

        // Valida redirecionamento HTTPS em produção
        if (environment.IsProduction())
        {
            var httpsRedirection = configuration.GetValue<bool?>("HttpsRedirection:Enabled");
            if (httpsRedirection == false)
                errors.Add("HTTPS redirection must be enabled in production environment");
        }

        // Valida AllowedHosts
        var allowedHosts = configuration.GetValue<string>("AllowedHosts");
        if (environment.IsProduction() && allowedHosts == "*")
            errors.Add("AllowedHosts must be restricted to specific domains in production (not '*')");

        // Lança erros agregados se houver
        if (errors.Count > 0)
        {
            var errorMessage = "Security configuration validation failed:\n" + string.Join("\n", errors.Select(e => $"- {e}"));
            throw new InvalidOperationException(errorMessage);
        }
    }

    public static IServiceCollection AddCorsPolicy(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);
        // Registra opções de CORS usando AddOptions<>()
        services.AddOptions<CorsOptions>()
            .Configure<IConfiguration>((opts, config) =>
            {
                config.GetSection(CorsOptions.SectionName).Bind(opts);
            })
            .ValidateOnStart();

        // Obtém opções de CORS para uso imediato na configuração da política
        var corsOptions = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();
        corsOptions.Validate();

        services.AddCors(options =>
        {
            options.AddPolicy("DefaultPolicy", policy =>
            {
                // Configura origens permitidas
                if (corsOptions.AllowedOrigins.Contains("*"))
                {
                    // Só permite coringa em desenvolvimento
                    if (environment.IsDevelopment())
                    {
                        // AllowAnyOrigin() é incompatível com AllowCredentials()
                        if (corsOptions.AllowCredentials)
                        {
                            // Usa SetIsOriginAllowed para permitir qualquer origem com credenciais
                            policy.SetIsOriginAllowed(_ => true);
                        }
                        else
                        {
                            policy.AllowAnyOrigin();
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Wildcard CORS origin (*) is not allowed in production environments for security reasons.");
                    }
                }
                else
                {
                    policy.WithOrigins([.. corsOptions.AllowedOrigins]);
                }

                // Configura métodos permitidos
                if (corsOptions.AllowedMethods.Contains("*"))
                {
                    policy.AllowAnyMethod();
                }
                else
                {
                    policy.WithMethods([.. corsOptions.AllowedMethods]);
                }

                // Configura cabeçalhos permitidos
                if (corsOptions.AllowedHeaders.Contains("*"))
                {
                    policy.AllowAnyHeader();
                }
                else
                {
                    policy.WithHeaders([.. corsOptions.AllowedHeaders]);
                }

                // Configura credenciais (apenas se explicitamente habilitado)
                if (corsOptions.AllowCredentials)
                {
                    policy.AllowCredentials();
                }

                // Define tempo máximo de cache do preflight
                policy.SetPreflightMaxAge(TimeSpan.FromSeconds(corsOptions.PreflightMaxAge));
            });
        });

        return services;
    }

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
        if (!environment.IsEnvironment("Testing"))
        {
            services.AddKeycloakAuthentication(configuration, environment);
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
                        context.Response.Headers.Append("X-Debug-Auth-Failure", context.Exception.Message.Replace(Environment.NewLine, " "));
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
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Sistema de permissões type-safe (único e centralizado)
        services.AddPermissionBasedAuthorization();

        // Adiciona políticas especiais que precisam de handlers customizados
        services.AddAuthorizationBuilder()
            .AddPolicy("SelfOrAdmin", policy =>
                policy.AddRequirements(new SelfOrAdminRequirement()))
            .AddPolicy("AdminOnly", policy =>
                policy.RequireRole("admin", "super-admin"))
            .AddPolicy("SuperAdminOnly", policy =>
                policy.RequireRole("super-admin"));

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

        // Extrai roles do realm_access
        if (jwtToken.Payload.TryGetValue("realm_access", out var realmObj) &&
            realmObj is IDictionary<string, object> realmDict &&
            realmDict.TryGetValue("roles", out var realmRoles) &&
            realmRoles is IEnumerable<object> realmRolesList)
        {
            foreach (var role in realmRolesList.OfType<string>())
            {
                roleClaims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        // Extrai roles do resource_access para o cliente específico
        if (jwtToken.Payload.TryGetValue("resource_access", out var resourceObj) &&
            resourceObj is IDictionary<string, object> resourceDict &&
            resourceDict.TryGetValue(clientId, out var clientObj) &&
            clientObj is IDictionary<string, object> clientDict &&
            clientDict.TryGetValue("roles", out var clientRoles) &&
            clientRoles is IEnumerable<object> clientRolesList)
        {
            foreach (var role in clientRolesList.OfType<string>())
            {
                roleClaims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        return roleClaims;
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
