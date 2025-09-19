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
    /// Validates all security-related configurations to prevent misconfiguration in production.
    /// </summary>
    /// <param name="configuration">Application configuration</param>
    /// <param name="environment">Hosting environment</param>
    /// <exception cref="InvalidOperationException">Thrown when security configuration is invalid</exception>
    public static void ValidateSecurityConfiguration(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var errors = new List<string>();

        // Validate CORS configuration
        try
        {
            var corsOptions = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();
            corsOptions.Validate();

            // Additional production-specific CORS validations
            if (environment.IsProduction())
            {
                if (corsOptions.AllowedOrigins.Contains("*"))
                    errors.Add("Wildcard CORS origin (*) is not allowed in production environment");

                if (corsOptions.AllowedOrigins.Any(o => o.StartsWith("http://", StringComparison.OrdinalIgnoreCase)))
                    errors.Add("HTTP origins are not recommended in production environment - use HTTPS");

                if (corsOptions.AllowCredentials && corsOptions.AllowedOrigins.Count > 5)
                    errors.Add("Having many allowed origins with credentials enabled increases security risk");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"CORS configuration error: {ex.Message}");
        }

        // Validate Keycloak configuration (if not in Testing environment)
        if (!environment.IsEnvironment("Testing"))
        {
            try
            {
                var keycloakOptions = configuration.GetSection(KeycloakOptions.SectionName).Get<KeycloakOptions>() ?? new KeycloakOptions();
                ValidateKeycloakOptions(keycloakOptions);

                // Additional production-specific validations
                if (environment.IsProduction())
                {
                    if (!keycloakOptions.RequireHttpsMetadata)
                        errors.Add("RequireHttpsMetadata should be true in production environment");

                    if (keycloakOptions.BaseUrl?.StartsWith("http://", StringComparison.OrdinalIgnoreCase) == true)
                        errors.Add("Keycloak BaseUrl should use HTTPS in production environment");

                    if (keycloakOptions.ClockSkew.TotalMinutes > 5)
                        errors.Add("Keycloak ClockSkew should be minimal (≤5 minutes) in production for better security");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Keycloak configuration error: {ex.Message}");
            }
        }

        // Validate Rate Limiting configuration
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
                        errors.Add("Anonymous rate limits must be positive values");

                    if (environment.IsProduction() && anonMinute > 100)
                        errors.Add("Anonymous rate limits should be conservative in production (≤100 req/min)");
                }

                if (authenticatedLimits.Exists())
                {
                    var authMinute = authenticatedLimits.GetValue<int>("RequestsPerMinute");
                    var authHour = authenticatedLimits.GetValue<int>("RequestsPerHour");

                    if (authMinute <= 0 || authHour <= 0)
                        errors.Add("Authenticated rate limits must be positive values");
                }
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Rate limiting configuration error: {ex.Message}");
        }

        // Validate HTTPS redirection in production
        if (environment.IsProduction())
        {
            var httpsRedirection = configuration.GetValue<bool?>("HttpsRedirection:Enabled");
            if (httpsRedirection == false)
                errors.Add("HTTPS redirection should be enabled in production environment");
        }

        // Validate AllowedHosts
        var allowedHosts = configuration.GetValue<string>("AllowedHosts");
        if (environment.IsProduction() && allowedHosts == "*")
            errors.Add("AllowedHosts should be restricted to specific domains in production (not '*')");

        // Throw aggregated errors if any
        if (errors.Any())
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
        // Register CORS options using AddOptions<>()
        services.AddOptions<CorsOptions>()
            .Configure<IConfiguration>((opts, config) =>
            {
                config.GetSection(CorsOptions.SectionName).Bind(opts);
            })
            .ValidateOnStart();

        // Get CORS options for immediate use in policy configuration
        var corsOptions = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();
        corsOptions.Validate();

        services.AddCors(options =>
        {
            options.AddPolicy("DefaultPolicy", policy =>
            {
                // Configure allowed origins
                if (corsOptions.AllowedOrigins.Contains("*"))
                {
                    // Only allow wildcard in development
                    if (environment.IsDevelopment())
                    {
                        policy.AllowAnyOrigin();
                    }
                    else
                    {
                        throw new InvalidOperationException("Wildcard CORS origin (*) is not allowed in production environments for security reasons.");
                    }
                }
                else
                {
                    policy.WithOrigins(corsOptions.AllowedOrigins.ToArray());
                }

                // Configure allowed methods
                if (corsOptions.AllowedMethods.Contains("*"))
                {
                    policy.AllowAnyMethod();
                }
                else
                {
                    policy.WithMethods(corsOptions.AllowedMethods.ToArray());
                }

                // Configure allowed headers
                if (corsOptions.AllowedHeaders.Contains("*"))
                {
                    policy.AllowAnyHeader();
                }
                else
                {
                    policy.WithHeaders(corsOptions.AllowedHeaders.ToArray());
                }

                // Configure credentials (only if explicitly enabled)
                if (corsOptions.AllowCredentials)
                {
                    policy.AllowCredentials();
                }

                // Set preflight cache max age
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
        // Register KeycloakOptions using AddOptions<>()
        services.AddOptions<KeycloakOptions>()
            .Configure<IConfiguration>((opts, config) =>
            {
                config.GetSection(KeycloakOptions.SectionName).Bind(opts);
            })
            .ValidateOnStart();

        // Get KeycloakOptions for immediate use in configuration
        var keycloakOptions = configuration.GetSection(KeycloakOptions.SectionName).Get<KeycloakOptions>() ?? new KeycloakOptions();

        // Validate Keycloak configuration
        ValidateKeycloakOptions(keycloakOptions);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = keycloakOptions.AuthorityUrl;
                options.Audience = keycloakOptions.ClientId;
                options.RequireHttpsMetadata = keycloakOptions.RequireHttpsMetadata;
                    
                    // Enhanced token validation parameters
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = keycloakOptions.ValidateIssuer,
                        ValidateAudience = keycloakOptions.ValidateAudience,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = keycloakOptions.ClockSkew,
                        RoleClaimType = "roles", // Keycloak uses 'roles' claim
                        NameClaimType = "preferred_username" // Keycloak preferred username claim
                    };

                    // Add events for logging authentication issues
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

            // Log the effective Keycloak configuration (without secrets)
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

        // Register authorization handlers
        services.AddScoped<IAuthorizationHandler, SelfOrAdminHandler>();

        return services;
    }
}