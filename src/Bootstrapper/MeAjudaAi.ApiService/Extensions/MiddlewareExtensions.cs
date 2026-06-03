using MeAjudaAi.ApiService.Middlewares;
using System.Diagnostics.CodeAnalysis;

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
