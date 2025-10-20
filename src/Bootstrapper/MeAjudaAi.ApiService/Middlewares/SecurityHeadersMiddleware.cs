namespace MeAjudaAi.ApiService.Middlewares;

/// <summary>
/// Middleware para adicionar cabeçalhos de segurança com impacto mínimo na performance
/// </summary>
public class SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment environment)
{
    private readonly RequestDelegate _next = next;
    private readonly bool _isDevelopment = environment.IsDevelopment();

    // Valores de cabeçalho pré-computados para evitar concatenação de strings a cada requisição
    private static readonly KeyValuePair<string, string>[] StaticHeaders =
    [
        new("X-Content-Type-Options", "nosniff"),
        new("X-Frame-Options", "DENY"),
        new("X-XSS-Protection", "1; mode=block"),
        new("Referrer-Policy", "strict-origin-when-cross-origin"),
        new("Permissions-Policy", "geolocation=(), microphone=(), camera=()"),
        new("Content-Security-Policy",
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data: https:; " +
            "font-src 'self'; " +
            "connect-src 'self'; " +
            "frame-ancestors 'none';")
    ];

    private const string HstsHeader = "max-age=31536000; includeSubDomains";

    // Cabeçalhos para remover - usando array para iteração mais rápida
    private static readonly string[] HeadersToRemove = ["Server", "X-Powered-By", "X-AspNet-Version"];

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // Adiciona cabeçalhos de segurança estáticos eficientemente
        foreach (var header in StaticHeaders)
        {
            headers.Append(header.Key, header.Value);
        }

        // HSTS apenas em produção e HTTPS - usando verificação de ambiente em cache
        if (context.Request.IsHttps && !_isDevelopment)
        {
            headers.Append("Strict-Transport-Security", HstsHeader);
        }

        // Remove cabeçalhos de exposição de informações eficientemente
        foreach (var headerName in HeadersToRemove)
        {
            headers.Remove(headerName);
        }

        await _next(context);
    }
}
