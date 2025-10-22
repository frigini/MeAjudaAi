using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

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
        // Serviços apenas para produção
        if (environment.IsProduction())
        {
            services.AddProductionServices();
        }
        // Serviços para desenvolvimento
        else if (environment.IsDevelopment())
        {
            services.AddDevelopmentServices();
        }
        // Serviços para testes de integração
        else if (environment.IsEnvironment("Testing"))
        {
            services.AddTestingServices();
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
    private static IServiceCollection AddDevelopmentServices(this IServiceCollection services)
    {
        // Documentação Swagger verbose apenas em desenvolvimento
        services.AddDevelopmentDocumentation();

        return services;
    }

    /// <summary>
    /// Adiciona serviços específicos para ambiente de testes
    /// </summary>
    private static IServiceCollection AddTestingServices(this IServiceCollection services)
    {
        // Para o ambiente de Testing, configuramos apenas os serviços básicos
        // A autenticação será configurada pelos testes individuais usando WebApplicationFactory
        return services;
    }

    /// <summary>
    /// Adiciona serviços específicos para ambiente de produção
    /// </summary>
    private static IServiceCollection AddProductionServices(this IServiceCollection services)
    {
        // Configurações de HSTS para produção
        services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays(365); // 1 ano
        });

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
        // HSTS (HTTP Strict Transport Security) deve vir antes do redirecionamento HTTPS
        app.UseHsts();

        // Middleware de redirecionamento HTTPS obrigatório em produção
        app.UseHttpsRedirection();

        // Headers de segurança mais restritivos em produção
        app.Use(async (context, next) =>
        {
            // Remove headers que podem expor informações do servidor
            context.Response.Headers.Remove("Server");

            // Adiciona headers de segurança essenciais para produção
            context.Response.Headers.Append("X-Production", "true");

            // Strict-Transport-Security (redundante com UseHsts, mas garante configuração explícita)
            if (!context.Response.Headers.ContainsKey("Strict-Transport-Security"))
            {
                context.Response.Headers.Append("Strict-Transport-Security",
                    "max-age=31536000; includeSubDomains; preload");
            }

            // X-Content-Type-Options: previne MIME type sniffing
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

            // X-Frame-Options: previne clickjacking
            context.Response.Headers.Append("X-Frame-Options", "DENY");

            // Referrer-Policy: controla informações de referrer
            context.Response.Headers.Append("Referrer-Policy", "no-referrer");

            // X-XSS-Protection: habilitado em navegadores legados (opcional, mas recomendado)
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

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
    public IReadOnlyList<string> AllowedHosts { get; set; } = [];
}

/// <summary>
/// Opções para o esquema de autenticação de teste
/// </summary>
public class TestAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// Usuário padrão para testes
    /// </summary>
    public string DefaultUserId { get; set; } = "test-user-id";
    
    /// <summary>
    /// Nome do usuário padrão para testes
    /// </summary>
    public string DefaultUserName { get; set; } = "test-user";
}

/// <summary>
/// Handler de autenticação simplificado para ambiente de teste
/// </summary>
public class TestAuthenticationHandler : AuthenticationHandler<TestAuthenticationSchemeOptions>
{
    public TestAuthenticationHandler(IOptionsMonitor<TestAuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Para testes, sempre autenticamos com um usuário padrão
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Name, "test-user"),
            new Claim(ClaimTypes.Role, "user")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
