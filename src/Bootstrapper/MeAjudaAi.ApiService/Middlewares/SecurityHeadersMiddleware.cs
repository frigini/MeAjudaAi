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

    // Nomes dos Cabeçalhos
    private const string XFrameOptions = "X-Frame-Options";
    private const string XContentTypeOptions = "X-Content-Type-Options";
    private const string ReferrerPolicy = "Referrer-Policy";
    private const string XPoweredBy = "X-Powered-By";

    // Valores dos Cabeçalhos
    private const string Deny = "DENY";
    private const string NoSniff = "nosniff";
    private const string StrictOriginWhenCrossOrigin = "strict-origin-when-cross-origin";

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting((state) =>
        {
            var ctx = (HttpContext)state;
            logger?.LogTrace("Adding security headers to response via OnStarting.");

            // Adiciona headers apenas se não existirem
            
            // Impede que o site seja emoldurado (clickjacking)
            if (!ctx.Response.Headers.ContainsKey(XFrameOptions))
            {
                ctx.Response.Headers.Append(XFrameOptions, Deny);
            }

            // Impede que o navegador tente adivinhar o tipo de conteúdo (MIME sniffing)
            if (!ctx.Response.Headers.ContainsKey(XContentTypeOptions))
            {
                ctx.Response.Headers.Append(XContentTypeOptions, NoSniff);
            }

            // Controla quanta informação de referência é enviada
            if (!ctx.Response.Headers.ContainsKey(ReferrerPolicy))
            {
                ctx.Response.Headers.Append(ReferrerPolicy, StrictOriginWhenCrossOrigin);
            }

            // Remove o cabeçalho que identifica a tecnologia do servidor
            ctx.Response.Headers.Remove(XPoweredBy);

            return Task.CompletedTask;
        }, context);

        await _next(context);
    }
}
