using MeAjudaAi.ApiService.Middlewares;

namespace MeAjudaAi.ApiService.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseApiMiddlewares(this IApplicationBuilder app)
    {
        // Cabeçalhos de segurança (no início do pipeline)
        app.UseMiddleware<SecurityHeadersMiddleware>();

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
