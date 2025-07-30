namespace MeAjudaAi.ApiService.Middlewares;

public class SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment environment)
{
    private readonly RequestDelegate _next = next;
    private readonly IWebHostEnvironment _environment = environment;

    public async Task InvokeAsync(HttpContext context)
    {
        // Adiciona headers de segurança - usando Append para evitar ASP0019
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");

        // HSTS apenas em produção e HTTPS
        if (context.Request.IsHttps && !_environment.IsDevelopment())
        {
            context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        }

        // CSP básico - ajuste conforme necessário
        var csp = "default-src 'self'; " +
                  "script-src 'self' 'unsafe-inline'; " +
                  "style-src 'self' 'unsafe-inline'; " +
                  "img-src 'self' data: https:; " +
                  "font-src 'self'; " +
                  "connect-src 'self'; " +
                  "frame-ancestors 'none';";

        context.Response.Headers.Append("Content-Security-Policy", csp);

        // Remove headers que expõem informações - usando TryGetValue para evitar warnings
        if (context.Response.Headers.TryGetValue("Server", out _))
            context.Response.Headers.Remove("Server");

        if (context.Response.Headers.TryGetValue("X-Powered-By", out _))
            context.Response.Headers.Remove("X-Powered-By");

        if (context.Response.Headers.TryGetValue("X-AspNet-Version", out _))
            context.Response.Headers.Remove("X-AspNet-Version");

        await _next(context);
    }
}