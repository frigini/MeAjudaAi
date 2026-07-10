using MeAjudaAi.ApiService.Middlewares;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.ApiService.Extensions;

/// <summary>
/// Extension methods para registrar o middleware CSP.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ContentSecurityPolicyMiddlewareExtensions
{
    public static IApplicationBuilder UseContentSecurityPolicy(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ContentSecurityPolicyMiddleware>();
    }
}
