namespace MeAjudaAi.ApiService.Middleware;

/// <summary>
/// Middleware para adicionar Content Security Policy headers.
/// Protege contra XSS, data injection e clickjacking.
/// </summary>
public class ContentSecurityPolicyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ContentSecurityPolicyMiddleware> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _cspPolicy;

    public ContentSecurityPolicyMiddleware(
        RequestDelegate next,
        ILogger<ContentSecurityPolicyMiddleware> logger,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
        _cspPolicy = BuildCspPolicy(environment, configuration);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Adicionar headers de CSP
        context.Response.Headers.Append("Content-Security-Policy", _cspPolicy);
        
        // Adicionar CSP Report-Only para testes (comentado para produção)
        // context.Response.Headers.Append("Content-Security-Policy-Report-Only", _cspPolicy);

        // Adicionar headers de segurança adicionais
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");

        _logger.LogDebug("CSP headers applied to response");

        await _next(context);
    }

    private static string BuildCspPolicy(IWebHostEnvironment environment, IConfiguration configuration)
    {
        var isDevelopment = environment.IsDevelopment();

        // Construir connect-src dinamicamente
        string connectSrc;
        if (isDevelopment)
        {
            // Em desenvolvimento, usar localhost
            connectSrc = "connect-src 'self' https://localhost:7001 http://localhost:8080 ws://localhost:* wss://localhost:*";
        }
        else
        {
            // Em produção, usar configurações
            var keycloakAuthority = configuration["Keycloak:Authority"] ?? "";
            var apiBaseUrl = configuration["ApiBaseUrl"] ?? "";
            var websocketUrl = configuration["WebSocketUrl"] ?? "";
            
            var origins = new List<string> { "'self'" };
            
            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
                origins.Add(apiBaseUrl);
            
            if (!string.IsNullOrWhiteSpace(keycloakAuthority))
                origins.Add(keycloakAuthority);
            
            if (!string.IsNullOrWhiteSpace(websocketUrl))
                origins.Add(websocketUrl);
            
            connectSrc = $"connect-src {string.Join(" ", origins)}";
        }

        // Base policy - muito restritivo
        var policy = new List<string>
        {
            // Padrão: bloquear tudo que não for explicitamente permitido
            "default-src 'self'",

            // Scripts: permitir self e o runtime do Blazor
            "script-src 'self' 'wasm-unsafe-eval'", // wasm-unsafe-eval necessário para Blazor WASM

            // Estilos: permitir self e estilos inline (necessário para MudBlazor)
            "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com",

            // Fonts: permitir self e Google Fonts
            "font-src 'self' https://fonts.gstatic.com data:",

            // Imagens: permitir self e data URIs
            "img-src 'self' data: https:",

            // Conexões (AJAX/fetch): permitir API e Keycloak - dinâmico
            connectSrc,

            // Mídia: bloquear tudo
            "media-src 'none'",

            // Objetos/Embeds: bloquear tudo
            "object-src 'none'",

            // Base URI: restringir a self
            "base-uri 'self'",

            // Formulários: permitir apenas self
            "form-action 'self'",

            // Ancestrais de frame: negar (prevenir clickjacking)
            "frame-ancestors 'none'",

            // Atualizar requisições inseguras em produção
            isDevelopment ? "" : "upgrade-insecure-requests"
        };

        // Remove empty entries
        var finalPolicy = string.Join("; ", policy.Where(p => !string.IsNullOrWhiteSpace(p)));

        return finalPolicy;
    }
}

/// <summary>
/// Extension methods para registrar o middleware CSP.
/// </summary>
public static class ContentSecurityPolicyMiddlewareExtensions
{
    public static IApplicationBuilder UseContentSecurityPolicy(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ContentSecurityPolicyMiddleware>();
    }
}
