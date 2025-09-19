using MeAjudaAi.ApiService.Handlers;

namespace MeAjudaAi.ApiService.Extensions;

/// <summary>
/// Extensões para registro de middlewares específicos por ambiente
/// </summary>
public static class EnvironmentSpecificExtensions
{
    /// <summary>
    /// Configura serviços específicos por ambiente
    /// </summary>
    public static IServiceCollection AddEnvironmentSpecificServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        // Serviços para desenvolvimento, testes e integração
        if (environment.IsDevelopment() || 
            environment.IsEnvironment("Testing") || 
            environment.IsEnvironment("Integration"))
        {
            services.AddDevelopmentServices(configuration, environment);
        }
        
        // Serviços apenas para produção
        if (environment.IsProduction())
        {
            services.AddProductionServices();
        }

        return services;
    }

    /// <summary>
    /// Configura middlewares específicos por ambiente
    /// </summary>
    public static IApplicationBuilder UseEnvironmentSpecificMiddlewares(
        this IApplicationBuilder app,
        IWebHostEnvironment environment)
    {
        // Middlewares apenas para desenvolvimento e testes
        if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
        {
            app.UseDevelopmentMiddlewares();
        }
        
        // Middlewares apenas para produção
        if (environment.IsProduction())
        {
            app.UseProductionMiddlewares();
        }

        return app;
    }

    /// <summary>
    /// Adiciona serviços específicos para ambiente de desenvolvimento
    /// </summary>
    private static IServiceCollection AddDevelopmentServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        // Documentação Swagger verbose apenas em desenvolvimento
        services.AddDevelopmentDocumentation();
        
        // TestAuthentication para ambientes de teste
        if (environment.IsEnvironment("Testing") || environment.IsEnvironment("Integration"))
        {
            services.AddTestAuthentication();
        }

        return services;
    }

    /// <summary>
    /// Adiciona serviços específicos para ambiente de produção
    /// </summary>
    private static IServiceCollection AddProductionServices(this IServiceCollection services)
    {
        // Configurações de produção mais restritivas
        services.Configure<SecurityOptions>(options =>
        {
            // Configurações de segurança específicas de produção
            options.EnforceHttps = true;
            options.EnableStrictTransportSecurity = true;
        });

        return services;
    }

    /// <summary>
    /// Configura middlewares específicos para desenvolvimento
    /// </summary>
    private static IApplicationBuilder UseDevelopmentMiddlewares(this IApplicationBuilder app)
    {
        // Middleware de developer exception page já é configurado pelo ASP.NET Core
        
        // Logging verboso apenas em desenvolvimento
        app.Use(async (context, next) =>
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogDebug("Development: Processing request {Method} {Path}", 
                context.Request.Method, context.Request.Path);
            
            await next();
        });

        return app;
    }

    /// <summary>
    /// Configura middlewares específicos para produção
    /// </summary>
    private static IApplicationBuilder UseProductionMiddlewares(this IApplicationBuilder app)
    {
        // Middleware de redirecionamento HTTPS obrigatório em produção
        app.UseHttpsRedirection();
        
        // Headers de segurança mais restritivos em produção
        app.Use(async (context, next) =>
        {
            // Headers de segurança adicionais para produção
            context.Response.Headers.Remove("Server");
            context.Response.Headers.Append("X-Production", "true");
            
            await next();
        });

        return app;
    }

    /// <summary>
    /// Adiciona documentação Swagger detalhada apenas para desenvolvimento
    /// </summary>
    private static IServiceCollection AddDevelopmentDocumentation(this IServiceCollection services)
    {
        // Configurações de documentação específicas para desenvolvimento
        // Isso poderia incluir exemplos mais detalhados, schemas completos, etc.
        return services;
    }

    /// <summary>
    /// Adiciona autenticação de teste para ambiente de testing
    /// </summary>
    private static IServiceCollection AddTestAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "AspireTest";
                options.DefaultChallengeScheme = "AspireTest";
                options.DefaultScheme = "AspireTest";
            })
            .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthenticationHandler>(
                "AspireTest", options => { });
        
        return services;
    }
}

/// <summary>
/// Opções de segurança específicas por ambiente
/// </summary>
public class SecurityOptions
{
    public bool EnforceHttps { get; set; }
    public bool EnableStrictTransportSecurity { get; set; }
    public string[] AllowedHosts { get; set; } = [];
}