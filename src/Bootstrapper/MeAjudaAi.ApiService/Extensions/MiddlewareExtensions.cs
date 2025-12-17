using MeAjudaAi.ApiService.Middlewares;

namespace MeAjudaAi.ApiService.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseApiMiddlewares(this IApplicationBuilder app)
    {
        // ForwardedHeaders deve ser o primeiro para popular corretamente RemoteIpAddress para rate limiting
        // Processa cabeçalhos X-Forwarded-* de proxies reversos (load balancers, nginx, etc.)
        app.UseForwardedHeaders();

        // Cabeçalhos de segurança (no início do pipeline)
        app.UseMiddleware<SecurityHeadersMiddleware>();

        // Verificação de segurança de compressão (previne CRIME/BREACH)
        app.UseMiddleware<CompressionSecurityMiddleware>();

        // Compressão de resposta
        app.UseResponseCompression();

        // Arquivos estáticos com cache
        app.UseMiddleware<StaticFilesMiddleware>();

        // Log de requisições
        app.UseMiddleware<RequestLoggingMiddleware>();

        // Limitação de taxa (rate limiting)
        app.UseMiddleware<RateLimitingMiddleware>();

        return app;
    }
}
