using MeAjudaAi.ApiService.Handlers;
using MeAjudaAi.ApiService.Options;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace MeAjudaAi.ApiService.Extensions;

public static class SecurityExtensions
{
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

    /// <summary>
    /// Valida todas as configurações relacionadas à segurança para evitar erros em produção.
    /// </summary>
    /// <param name="configuration">Configuração da aplicação</param>
    /// <param name="environment">Ambiente de hospedagem</param>
    /// <exception cref="InvalidOperationException">Lançada quando a configuração de segurança é inválida</exception>
    public static void ValidateSecurityConfiguration(IConfiguration configuration, IWebHostEnvironment environment)
    {
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
                    errors.Add("Origem CORS coringa (*) não é permitida em ambiente de produção");

                if (corsOptions.AllowedOrigins.Any(o => o.StartsWith("http://", StringComparison.OrdinalIgnoreCase)))
                    errors.Add("Origens HTTP não são recomendadas em produção - use HTTPS");

                if (corsOptions.AllowCredentials && corsOptions.AllowedOrigins.Count > 5)
                    errors.Add("Muitas origens permitidas com credenciais habilitadas aumentam o risco de segurança");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Erro na configuração do CORS: {ex.Message}");
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
                        errors.Add("RequireHttpsMetadata deve ser true em ambiente de produção");

                    if (keycloakOptions.BaseUrl?.StartsWith("http://", StringComparison.OrdinalIgnoreCase) == true)
                        errors.Add("Keycloak BaseUrl deve usar HTTPS em ambiente de produção");

                    if (keycloakOptions.ClockSkew.TotalMinutes > 5)
                        errors.Add("Keycloak ClockSkew deve ser mínimo (≤5 minutos) em produção para maior segurança");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Erro na configuração do Keycloak: {ex.Message}");
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
                        errors.Add("Limites de requisições anônimas devem ser valores positivos");

                    if (environment.IsProduction() && anonMinute > 100)
                        errors.Add("Limites de requisições anônimas devem ser conservadores em produção (≤100 req/min)");
                }

                if (authenticatedLimits.Exists())
                {
                    var authMinute = authenticatedLimits.GetValue<int>("RequestsPerMinute");
                    var authHour = authenticatedLimits.GetValue<int>("RequestsPerHour");

                    if (authMinute <= 0 || authHour <= 0)
                        errors.Add("Limites de requisições autenticadas devem ser valores positivos");
                }
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Erro na configuração de rate limiting: {ex.Message}");
        }

        // Valida redirecionamento HTTPS em produção
        if (environment.IsProduction())
        {
            var httpsRedirection = configuration.GetValue<bool?>("HttpsRedirection:Enabled");
            if (httpsRedirection == false)
                errors.Add("Redirecionamento HTTPS deve estar habilitado em ambiente de produção");
        }

        // Valida AllowedHosts
        var allowedHosts = configuration.GetValue<string>("AllowedHosts");
        if (environment.IsProduction() && allowedHosts == "*")
            errors.Add("AllowedHosts deve ser restrito a domínios específicos em produção (não '*')");

        // Lança erros agregados se houver
        if (errors.Any())
        {
            var errorMessage = "Falha na validação da configuração de segurança:\n" + string.Join("\n", errors.Select(e => $"- {e}"));
            throw new InvalidOperationException(errorMessage);
        }
    }

    public static IServiceCollection AddCorsPolicy(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
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
                        policy.AllowAnyOrigin();
                    }
                    else
                    {
                        throw new InvalidOperationException("Origem CORS coringa (*) não é permitida em ambientes de produção por motivos de segurança.");
                    }
                }
                else
                {
                    policy.WithOrigins(corsOptions.AllowedOrigins.ToArray());
                }

                // Configura métodos permitidos
                if (corsOptions.AllowedMethods.Contains("*"))
                {
                    policy.AllowAnyMethod();
                }
                else
                {
                    policy.WithMethods(corsOptions.AllowedMethods.ToArray());
                }

                // Configura cabeçalhos permitidos
                if (corsOptions.AllowedHeaders.Contains("*"))
                {
                    policy.AllowAnyHeader();
                }
                else
                {
                    policy.WithHeaders(corsOptions.AllowedHeaders.ToArray());
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
                        RoleClaimType = "roles", // Keycloak usa o claim 'roles'
                        NameClaimType = "preferred_username" // Claim de usuário preferencial do Keycloak
                    };

                    // Adiciona eventos para log de problemas de autenticação
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerHandler>>();
                            logger.LogWarning("JWT authentication failed: {Exception}", context.Exception.Message);
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
                            var userId = context.Principal?.FindFirst("sub")?.Value;
                            logger.LogDebug("JWT token validated successfully for user: {UserId}", userId);
                            return Task.CompletedTask;
                        }
                    };
                });

            // Loga a configuração efetiva do Keycloak (sem segredos)
            using var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetRequiredService<ILogger<JwtBearerHandler>>();
            logger.LogInformation("Keycloak authentication configured - Authority: {Authority}, ClientId: {ClientId}, ValidateIssuer: {ValidateIssuer}", 
                keycloakOptions.AuthorityUrl, keycloakOptions.ClientId, keycloakOptions.ValidateIssuer);

        return services;
    }

    /// <summary>
    /// Configura políticas de autorização
    /// </summary>
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy =>
                policy.RequireRole("admin", "super-admin"))
            .AddPolicy("SuperAdminOnly", policy =>
                policy.RequireRole("super-admin"))
            .AddPolicy("UserManagement", policy =>
                policy.RequireRole("admin", "super-admin"))
            .AddPolicy("ServiceProviderAccess", policy =>
                policy.RequireRole("service-provider", "admin", "super-admin"))
            .AddPolicy("CustomerAccess", policy =>
                policy.RequireRole("customer", "admin", "super-admin"))
            .AddPolicy("SelfOrAdmin", policy =>
                policy.AddRequirements(new SelfOrAdminRequirement()));

        // Registra handlers de autorização
        services.AddScoped<IAuthorizationHandler, SelfOrAdminHandler>();

        return services;
    }
}