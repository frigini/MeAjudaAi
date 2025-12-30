using MeAjudaAi.ApiService.Middlewares;

namespace MeAjudaAi.ApiService.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseApiMiddlewares(this IApplicationBuilder app)
    {
        // Cabeçalhos de segurança (no início do pipeline)
        app.UseMiddleware<SecurityHeadersMiddleware>();

        // Limitação de taxa (rate limiting)
        app.UseMiddleware<RateLimitingMiddleware>();

        return app;
    }
}
