using System.Diagnostics.CodeAnalysis;
using MeAjudaAi.ApiService.Middlewares;

namespace MeAjudaAi.ApiService.Extensions;

[ExcludeFromCodeCoverage]
public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseApiMiddlewares(this IApplicationBuilder app)
    {
        // Cabeçalhos de segurança (no início do pipeline)
        app.UseMiddleware<SecurityHeadersMiddleware>();

        return app;
    }
}
