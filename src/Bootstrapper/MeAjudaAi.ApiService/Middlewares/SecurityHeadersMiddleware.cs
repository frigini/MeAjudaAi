using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.ApiService.Middlewares;

/// <summary>
/// Middleware para adicionar cabeçalhos de segurança (Hardening).
/// Atua como um fallback seguro, garantindo que cabeçalhos essenciais estejam presentes
/// sem sobrescrever configurações específicas de ambiente ou middlewares especializados (como CSP).
/// </summary>
public sealed class SecurityHeadersMiddleware(
    RequestDelegate next,
    ILogger<SecurityHeadersMiddleware>? logger = null)
{
    private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));

    public async Task InvokeAsync(HttpContext context)
    {
        logger?.LogTrace("Adding security headers to response.");

        // Adiciona headers apenas se não existirem (evita duplicidade com EnvironmentSpecificExtensions)
        
        // Impede que o site seja emoldurado (clickjacking)
        if (!context.Response.Headers.ContainsKey("X-Frame-Options"))
        {
            context.Response.Headers.Append("X-Frame-Options", "DENY");
        }

        // Impede que o navegador tente adivinhar o tipo de conteúdo (MIME sniffing)
        if (!context.Response.Headers.ContainsKey("X-Content-Type-Options"))
        {
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        }

        // Controla quanta informação de referência é enviada
        if (!context.Response.Headers.ContainsKey("Referrer-Policy"))
        {
            // Alinhado com o padrão de produção: strict-origin-when-cross-origin é um bom balanço
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        }

        // Nota: Content-Security-Policy (CSP) é gerenciado pelo ContentSecurityPolicyMiddleware
        // Nota: Permissions-Policy é gerenciado pelo ContentSecurityPolicyMiddleware (registrado antes deste middleware)
        // para permitir configuração dinâmica e evitar conflitos.

        // Remove o cabeçalho que identifica a tecnologia do servidor
        context.Response.Headers.Remove("X-Powered-By");

        await _next(context);
    }
}
