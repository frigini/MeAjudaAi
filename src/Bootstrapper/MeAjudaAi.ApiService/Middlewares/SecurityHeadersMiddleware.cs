using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.ApiService.Middlewares;

/// <summary>
/// Middleware para adicionar cabeçalhos de segurança (Hardening).
/// </summary>
public sealed class SecurityHeadersMiddleware(
    RequestDelegate next, 
    IWebHostEnvironment environment,
    ILogger<SecurityHeadersMiddleware>? logger = null)
{
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(environment);

        logger?.LogTrace("Adding security headers to response.");
        // Impede que o site seja emoldurado (clickjacking)
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        // Impede que o navegador tente adivinhar o tipo de conteúdo (MIME sniffing)
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // Controla quanta informação de referência é enviada
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Define quais recursos do navegador podem ser usados (Câmera, Microfone, etc.)
        context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=(self)");

        // Content Security Policy (CSP) - Configuração base restritiva para APIs
        // Permite scripts e estilos apenas da mesma origem e de fontes confiáveis (se houver)
        context.Response.Headers.Append("Content-Security-Policy", 
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data:; " +
            "font-src 'self'; " +
            "connect-src 'self' https://*.Betenbough.com; " + // Permite chamadas para domínios Betenbough
            "frame-ancestors 'none'; " +
            "form-action 'self';");

        // Remove o cabeçalho que identifica a tecnologia do servidor
        context.Response.Headers.Remove("X-Powered-By");

        await next(context);
    }
}
