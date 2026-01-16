namespace MeAjudaAi.ApiService.Middleware;

/// <summary>
/// Middleware para adicionar Content Security Policy headers.
/// Protege contra XSS, data injection e clickjacking.
/// </summary>
public class ContentSecurityPolicyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ContentSecurityPolicyMiddleware> _logger;
    private readonly string _cspPolicy;

    public ContentSecurityPolicyMiddleware(
        RequestDelegate next,
        ILogger<ContentSecurityPolicyMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _cspPolicy = BuildCspPolicy(environment);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add CSP headers
        context.Response.Headers.Append("Content-Security-Policy", _cspPolicy);
        
        // Add CSP Report-Only for testing (commented out for production)
        // context.Response.Headers.Append("Content-Security-Policy-Report-Only", _cspPolicy);

        // Add additional security headers
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");

        _logger.LogDebug("CSP headers applied to response");

        await _next(context);
    }

    private static string BuildCspPolicy(IWebHostEnvironment environment)
    {
        var isDevelopment = environment.IsDevelopment();

        // Base policy - muito restritivo
        var policy = new List<string>
        {
            // Default: block everything not explicitly allowed
            "default-src 'self'",

            // Scripts: allow self and Blazor framework
            "script-src 'self' 'wasm-unsafe-eval'", // wasm-unsafe-eval required for Blazor WASM

            // Styles: allow self and inline styles (required for MudBlazor)
            "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com",

            // Fonts: allow self and Google Fonts
            "font-src 'self' https://fonts.gstatic.com data:",

            // Images: allow self and data URIs
            "img-src 'self' data: https:",

            // Connect (AJAX/fetch): allow API and Keycloak
            "connect-src 'self' https://localhost:7001 http://localhost:8080 ws://localhost:* wss://localhost:*",

            // Media: block all
            "media-src 'none'",

            // Objects/Embeds: block all
            "object-src 'none'",

            // Base URI: restrict to self
            "base-uri 'self'",

            // Forms: allow self only
            "form-action 'self'",

            // Frame ancestors: deny (prevent clickjacking)
            "frame-ancestors 'none'",

            // Upgrade insecure requests in production
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
