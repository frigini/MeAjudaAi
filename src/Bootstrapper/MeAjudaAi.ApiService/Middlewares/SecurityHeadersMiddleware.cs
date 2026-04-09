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
        context.Response.OnStarting((state) =>
        {
            var ctx = (HttpContext)state;
            logger?.LogTrace("Adding security headers to response via OnStarting.");

            // Adiciona headers apenas se não existirem
            
            // Impede que o site seja emoldurado (clickjacking)
            if (!ctx.Response.Headers.ContainsKey("X-Frame-Options"))
            {
                ctx.Response.Headers.Append("X-Frame-Options", "DENY");
            }

            // Impede que o navegador tente adivinhar o tipo de conteúdo (MIME sniffing)
            if (!ctx.Response.Headers.ContainsKey("X-Content-Type-Options"))
            {
                ctx.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            }

            // Controla quanta informação de referência é enviada
            if (!ctx.Response.Headers.ContainsKey("Referrer-Policy"))
            {
                ctx.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            }

            // Remove o cabeçalho que identifica a tecnologia do servidor
            ctx.Response.Headers.Remove("X-Powered-By");

            return Task.CompletedTask;
        }, context);

        await _next(context);
    }
}
