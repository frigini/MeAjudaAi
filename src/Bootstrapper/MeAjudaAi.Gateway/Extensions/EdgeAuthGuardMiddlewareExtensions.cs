using MeAjudaAi.Gateway.Middlewares;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Gateway.Extensions;

/// <summary>
/// Métodos de extensão para facilitar a inclusão do middleware de proteção de autenticação de borda.
/// </summary>
[ExcludeFromCodeCoverage]
public static class EdgeAuthGuardMiddlewareExtensions
{
    public static IApplicationBuilder UseEdgeAuthGuard(this IApplicationBuilder app)
    {
        return app.UseMiddleware<EdgeAuthGuardMiddleware>();
    }
}